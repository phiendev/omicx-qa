using Microsoft.AspNetCore.Mvc;
using Nest;
using Omicx.QA.Common.Responses;
using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.Elasticsearch.Factories;
using Omicx.QA.Elasticsearch.Requests;
using Omicx.QA.Entities.EmailReceive;
using Omicx.QA.MultiTenancy.Customs;
using Omicx.QA.Services.DynamicEntity;
using Omicx.QA.Services.Elastic;
using Omicx.QA.Services.Email.Dto;
using Omicx.QA.Services.Email.Request;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Omicx.QA.Services.Email;

[Route("api/app/email-receive")]
public class EmailReceiveAppService : ApplicationService, IEmailReceive
{
    private readonly ICurrentCustomTenant _currentCustomTenant;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<EmailReceiveAppService> _logger;
    private readonly IRepository<EmailReceive, Guid> _emailReceiveRepository;
    private readonly IRepository<EmailReceiveAttribute, Guid> _emailReceiveAttributeRepository;
    private readonly IRepository<DynamicEntitySchema, Guid> _dynamicEntitySchemaRepository;
    private readonly IDynamicEntityElasticService _dynamicEntityElasticService;
    private readonly IDynamicEntityAppService _dynamicEntityAppService;
    private readonly IElasticClient _elasticClient;
    
    public EmailReceiveAppService(
        ICurrentCustomTenant currentCustomTenant,
        ICurrentUser currentUser,
        ILogger<EmailReceiveAppService> logger,
        IRepository<EmailReceive, Guid> emailReceiveRepository,
        IRepository<EmailReceiveAttribute, Guid> emailReceiveAttributeRepository,
        IRepository<DynamicEntitySchema, Guid> dynamicEntitySchemaRepository,
        IDynamicEntityElasticService dynamicEntityElasticService,
        IDynamicEntityAppService dynamicEntityAppService,
        IElasticClient elasticClient
    )
    {
        _currentCustomTenant = currentCustomTenant;
        _currentUser = currentUser;
        _logger = logger;
        _emailReceiveRepository = emailReceiveRepository;
        _emailReceiveAttributeRepository = emailReceiveAttributeRepository;
        _dynamicEntitySchemaRepository = dynamicEntitySchemaRepository;
        _dynamicEntityElasticService = dynamicEntityElasticService;
        _dynamicEntityAppService = dynamicEntityAppService;
        _elasticClient = elasticClient;
    }

    [HttpGet("get-email-receive/{id}")]
    public async Task<EmailReceiveDto> GetEmailReceive(Guid id)
    {
        try
        {
            var emailReceive = await _emailReceiveRepository.FindAsync(x => x.Id == id);
            if(emailReceive is null) throw new Exception($"Email receive with id {id} not found");
            
            var emailReceiveDto = ObjectMapper.Map<EmailReceive, EmailReceiveDto>(emailReceive);
            
            var emailReceiveAttributes = await _emailReceiveAttributeRepository.GetListAsync(x => x.EmailReceiveId == emailReceiveDto.Id);

            if (emailReceiveAttributes.Any())
            {
                emailReceiveDto.EmailReceiveAttributes = ObjectMapper.Map<List<EmailReceiveAttribute>, List<EmailReceiveAttributeDto>>(emailReceiveAttributes);
            }
            return emailReceiveDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get email receive failed.");
            throw new Exception("Get email receive failed."); 
        }
    }

    [HttpPost("get-email-receive-list-dynamic")]
    public async Task<DocumentResponse<IEnumerable<EmailReceiveDocument>>> GetEmailReceives(FilterDocumentRequest input)
    {
        try
        {
            var fil = FilterFactory.Filter(input.Payload);
            if (fil is null)
                return new DocumentResponse<IEnumerable<EmailReceiveDocument>>(Status: true, Message: "Filter is null.", Data: null);

            int? customTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();

            var req = PageRequest<EmailReceiveDocument>
                .Where(fil)
                .ForTenant(customTenantId)
                .Paging(input.CurrentPage, input.PageSize);

            var emailReceiveDocuments = await _elasticClient.QueryAsync<EmailReceiveDocument>(req);

            var dynamicEntitySchema = await _dynamicEntitySchemaRepository.FindAsync(x => x.EntityType == "email-receive");
            var schema = dynamicEntitySchema is not null ? await _elasticClient.GetByIdAsync<DynamicEntitySchemaDocument>(dynamicEntitySchema.Id, customTenantId) : null;
            
            return new DocumentResponse<IEnumerable<EmailReceiveDocument>>(
                Status: true,
                Message: "Lấy dữ liệu thành công.",
                Data: emailReceiveDocuments.Documents,
                RecordTotal: emailReceiveDocuments.Total,
                Schema: schema
            );
        }
        catch (Exception ex)
        {
            
            _logger.LogError(ex, "Get email receive failed.");
            throw new Exception("Get email receive failed."); 
        }
    }

    [HttpPost("create-email-receive")]
    public async Task<EmailReceiveDto> CreateEmailReceive(EmailReceiveRequest item)
    {
        try
        {
            #region Email Receive
            
            var add = ObjectMapper.Map<EmailReceiveRequest, EmailReceive>(item);
            add.TenantId = _currentCustomTenant.Id;
            add.CustomTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();
            add.CreatorId = _currentUser.Id;
            add.CreationTime = DateTime.Now;
            
            add.Links = _dynamicEntityAppService.GetDynamicLink("email-receive", add.EmailId);
            
            var result = await _emailReceiveRepository.InsertAsync(add, autoSave: true);
            
            #endregion
            
            #region Dynamic entity
            
            List<EmailReceiveAttribute> elasticEmailReceiveAttributes = new List<EmailReceiveAttribute>();
            
            if (item.DynamicAttributeValues is not null && item.DynamicAttributeValues.Any())
            {
                MapDynamicAttributes("email-receive", add, item.DynamicAttributeValues);
            }

            if (add.EmailReceiveAttributes is not null && add.EmailReceiveAttributes.Any())
            {
                foreach (var emailReceiveAttribute in add.EmailReceiveAttributes)
                {
                    emailReceiveAttribute.EmailReceiveId = result.Id;
                    emailReceiveAttribute.CreatorId = _currentUser.Id;
                    emailReceiveAttribute.CreationTime = DateTime.Now;
                }

                await _emailReceiveAttributeRepository.InsertManyAsync(add.EmailReceiveAttributes, autoSave: true);
                
                elasticEmailReceiveAttributes.AddRange(add.EmailReceiveAttributes);
            }
            
            #endregion

            await _dynamicEntityElasticService.UpsertEmailReceive(result, add.EmailReceiveAttributes);
            
            var response = ObjectMapper.Map<EmailReceive, EmailReceiveDto>(result);
            if (elasticEmailReceiveAttributes.Any())
            {
                response.EmailReceiveAttributes = ObjectMapper.Map<List<EmailReceiveAttribute>, List<EmailReceiveAttributeDto>>(elasticEmailReceiveAttributes);
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create email receive");
            throw new Exception("Failed to create email receive");
        }
    }

    [HttpPut("update-email-receive")]
    public async Task<EmailReceiveDto> UpdateEmailReceive(EmailReceiveRequest item)
    {
        try
        {
            var emailReceive = await _emailReceiveRepository.FindAsync(x => x.Id == item.Id);
            if (emailReceive is null) throw new Exception("EmailReceive not found");

            #region Email Receive

            var update = ObjectMapper.Map<EmailReceiveRequest, EmailReceive>(item);
            
            emailReceive.LastModifierId = _currentUser.Id;
            emailReceive.LastModificationTime = DateTime.Now;
            emailReceive.Links = _dynamicEntityAppService.GetDynamicLink("email-receive", update.EmailId);
            
            var result = await _emailReceiveRepository.UpdateAsync(emailReceive, autoSave: true);
            
            if (item.DynamicAttributeValues is not null && item.DynamicAttributeValues.Any())
            {
                MapDynamicAttributes("email-receive", update, item.DynamicAttributeValues);
            }

            #endregion
            
            #region Dynamic entity
            
            List<EmailReceiveAttribute> elasticEmailReceiveAttributes = new List<EmailReceiveAttribute>();
            
            var emailReceiveAttributes = await _emailReceiveAttributeRepository.GetListAsync(x => x.EmailReceiveId == item.Id);
            if (update.EmailReceiveAttributes is not null && update.EmailReceiveAttributes.Any())
            {
                var emailReceiveAttributesToAdd = update.EmailReceiveAttributes
                    .Where(a => emailReceiveAttributes.All(b => b.DynamicAttributeId != a.DynamicAttributeId))
                    .ToList();
                var emailReceiveAttributesToUpdate = emailReceiveAttributes
                    .Where(a => update.EmailReceiveAttributes.Any(b => b.DynamicAttributeId == a.DynamicAttributeId))
                    .ToList();
                
                if(emailReceiveAttributesToAdd.Any())
                {
                    foreach (var emailReceiveAttribute in emailReceiveAttributesToAdd)
                    {
                        emailReceiveAttribute.EmailReceiveId = result.Id;
                        emailReceiveAttribute.CreatorId = _currentUser.Id;
                        emailReceiveAttribute.CreationTime = DateTime.Now;
                    }

                    await _emailReceiveAttributeRepository.InsertManyAsync(emailReceiveAttributesToAdd, autoSave: true);
                    
                    elasticEmailReceiveAttributes.AddRange(emailReceiveAttributesToAdd);
                }

                if (emailReceiveAttributesToUpdate.Any())
                {
                    foreach (var emailReceiveAttribute in emailReceiveAttributesToUpdate)
                    {
                        var newEmailReceiveAttribute = update.EmailReceiveAttributes.FirstOrDefault(b => b.DynamicAttributeId == emailReceiveAttribute.DynamicAttributeId);
                        if (newEmailReceiveAttribute is not null) emailReceiveAttribute.Value = newEmailReceiveAttribute.Value;
                        emailReceiveAttribute.LastModifierId = _currentUser.Id;
                        emailReceiveAttribute.LastModificationTime = DateTime.Now;
                    }

                    await _emailReceiveAttributeRepository.UpdateManyAsync(emailReceiveAttributesToUpdate, autoSave: true);
                    
                    elasticEmailReceiveAttributes.AddRange(emailReceiveAttributesToUpdate);
                }
            }
            
            #endregion
            
            await _dynamicEntityElasticService.UpsertEmailReceive(result, elasticEmailReceiveAttributes);

            var response = ObjectMapper.Map<EmailReceive, EmailReceiveDto>(result);
            if (elasticEmailReceiveAttributes.Any())
            {
                response.EmailReceiveAttributes = ObjectMapper.Map<List<EmailReceiveAttribute>, List<EmailReceiveAttributeDto>>(elasticEmailReceiveAttributes);
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update email receive");
            throw new Exception("Failed to update email receive");
        }
    }

    [HttpDelete("delete-email-receive/{id}")]
    public async Task DeleteEmailReceive(Guid id)
    {
        try
        {
            var delete = await _emailReceiveRepository.FindAsync(x => x.Id == id);
            if(delete is null) throw new Exception("Not found");
            
            await _emailReceiveRepository.DeleteAsync(delete, autoSave: true);
            
            var emailReceiveAttributes = await _emailReceiveAttributeRepository.GetListAsync(x => x.EmailReceiveId == id);

            if (emailReceiveAttributes.Any())
            {
                await _emailReceiveAttributeRepository.DeleteManyAsync(emailReceiveAttributes, autoSave: true);
            }
            
            await _dynamicEntityElasticService.DeleteEmailReceive(id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete email receive");
            throw new Exception("Failed to delete email receive");
        }
    }
    
    private void MapDynamicAttributes(string entityType, EmailReceive emailReceive, IEnumerable<DynamicAttributeValue>? dynamicAttributeValues)
    {
        var dynamicAttributes = _dynamicEntityAppService.GetDynamicEntity(entityType).Result.DynamicAttributes;
        var emailReceiveAttributes = (from item in dynamicAttributeValues
                let dynamicAttribute = dynamicAttributes.FirstOrDefault(x => x.Id == item.DynamicAttributeId)
                                       ?? throw new Exception(
                                           $"Không tồn tại Dynamic Attribute [{item.DynamicAttributeId}]")
                select new EmailReceiveAttribute
                {
                    DynamicAttributeId = dynamicAttribute.Id,
                    Value = item.Value,
                    TenantId = emailReceive.TenantId,
                    CustomTenantId = emailReceive.CustomTenantId,
                    SystemName = dynamicAttribute.SystemName,
                    DisplayName = dynamicAttribute.DisplayName,
                    Type = dynamicAttribute.Type,
                })
            .ToList();
        
        emailReceive.EmailReceiveAttributes = emailReceiveAttributes;
    }
}