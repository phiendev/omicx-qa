using Microsoft.AspNetCore.Mvc;
using Nest;
using Omicx.QA.Common.Responses;
using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.Elasticsearch.Factories;
using Omicx.QA.Elasticsearch.Requests;
using Omicx.QA.Entities.CallAggregate;
using Omicx.QA.MultiTenancy.Customs;
using Omicx.QA.Services.Call.Dto;
using Omicx.QA.Services.Call.Request;
using Omicx.QA.Services.DynamicEntity;
using Omicx.QA.Services.Elastic;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Omicx.QA.Services.Call;

[Route("api/app/call-aggregate")]
public class CallAggregateAppService : ApplicationService, ICallAggregateAppService
{
    private readonly ICurrentCustomTenant _currentCustomTenant;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<CallAggregateAppService> _logger;
    private readonly IRepository<CallAggregate, Guid> _callAggregateRepository;
    private readonly IRepository<CallAggregateAttribute, Guid> _callAggregateAttributeRepository;
    private readonly IDynamicEntityElasticService _dynamicEntityElasticService;
    private readonly IDynamicEntityAppService _dynamicEntityAppService;
    private readonly IElasticClient _elasticClient;
    
    public CallAggregateAppService(
        ICurrentCustomTenant currentCustomTenant,
        ICurrentUser currentUser,
        ILogger<CallAggregateAppService> logger,
        IRepository<CallAggregate, Guid> callAggregateRepository,
        IRepository<CallAggregateAttribute, Guid> callAggregateAttributeRepository,
        IDynamicEntityElasticService dynamicEntityElasticService,
        IDynamicEntityAppService dynamicEntityAppService,
        IElasticClient elasticClient
        )
    {
        _currentCustomTenant = currentCustomTenant;
        _currentUser = currentUser;
        _logger = logger;
        _callAggregateRepository = callAggregateRepository;
        _callAggregateAttributeRepository = callAggregateAttributeRepository;
        _dynamicEntityElasticService = dynamicEntityElasticService;
        _dynamicEntityAppService = dynamicEntityAppService;
        _elasticClient = elasticClient;
    }

    [HttpGet("get-call-aggregate/{id}")]
    public async Task<CallAggregateDto> GetCallAggregate(Guid id)
    {
        try
        {
            var callAggregate = await _callAggregateRepository.FindAsync(x => x.Id == id);
            if(callAggregate is null) throw new Exception($"Call aggregate with id {id} not found");
            
            var callAggregateDto = ObjectMapper.Map<CallAggregate, CallAggregateDto>(callAggregate);
            
            var callAggregateAttributes = await _callAggregateAttributeRepository.GetListAsync(x => x.CallAggregateId == callAggregateDto.Id);

            if (callAggregateAttributes.Any())
            {
                callAggregateDto.CallAggregateAttributes = ObjectMapper.Map<List<CallAggregateAttribute>, List<CallAggregateAttributeDto>>(callAggregateAttributes);
            }
            return callAggregateDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get call aggregate failed.");
            throw new Exception("Get call aggregate failed."); 
        }
    }

    [HttpPost("get-call-aggregate-list-dynamic")]
    public async Task<DocumentResponse<IEnumerable<CallAggregateDocument>>> GetCallAggregates(FilterDocumentRequest input)
    {
        try
        {
            var fil = FilterFactory.Filter(input.Payload);
            if (fil is null)
                return new DocumentResponse<IEnumerable<CallAggregateDocument>>(Status: true, Message: "Filter is null.", Data: null);

            int? customTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();

            var req = PageRequest<CallAggregateDocument>
                .Where(fil)
                .ForTenant(customTenantId)
                .Paging(input.CurrentPage, input.PageSize);

            var callAggregateDocuments = await _elasticClient.QueryAsync<CallAggregateDocument>(req);

            var schema = await _elasticClient.GetByIdAsync<DynamicEntitySchemaDocument>("862838be-6586-c0f2-553e-3a16cdf702d9", customTenantId);

            return new DocumentResponse<IEnumerable<CallAggregateDocument>>(
                Status: true,
                Message: "Lấy dữ liệu thành công.",
                Data: callAggregateDocuments.Documents,
                RecordTotal: callAggregateDocuments.Total,
                Schema: schema
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Get call aggregate failed.");
            throw new Exception("Get call aggregate failed."); 
        }
    }

    
    [HttpPost("create-call-aggregate")]
    public async Task<CallAggregateDto> CreateCallAggregate(CallAggregateRequest item)
    {
        try
        {
            #region Call Aggregate
            
            var add = ObjectMapper.Map<CallAggregateRequest, CallAggregate>(item);
            add.TenantId = _currentCustomTenant.Id;
            add.CustomTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();
            add.CreatorId = _currentUser.Id;
            add.CreationTime = DateTime.Now;
            
            add.Links = _dynamicEntityAppService.GetDynamicLink("call-aggregate", add.CallId);
            
            var result = await _callAggregateRepository.InsertAsync(add, autoSave: true);
            
            #endregion
            
            #region Dynamic entity
            
            List<CallAggregateAttribute> elasticCallAggregateAttributes = new List<CallAggregateAttribute>();
            
            if (item.DynamicAttributeValues is not null && item.DynamicAttributeValues.Any())
            {
                MapDynamicAttributes("call-aggregate", add, item.DynamicAttributeValues);
            }

            if (add.CallAggregateAttributes is not null && add.CallAggregateAttributes.Any())
            {
                foreach (var callAggregateAttribute in add.CallAggregateAttributes)
                {
                    callAggregateAttribute.CallAggregateId = result.Id;
                    callAggregateAttribute.CreatorId = _currentUser.Id;
                    callAggregateAttribute.CreationTime = DateTime.Now;
                }

                await _callAggregateAttributeRepository.InsertManyAsync(add.CallAggregateAttributes, autoSave: true);
                
                elasticCallAggregateAttributes.AddRange(add.CallAggregateAttributes);
            }
            
            #endregion

            await _dynamicEntityElasticService.UpsertCallAggregate(result, add.CallAggregateAttributes);
            
            var response = ObjectMapper.Map<CallAggregate, CallAggregateDto>(result);
            if (elasticCallAggregateAttributes.Any())
            {
                response.CallAggregateAttributes = ObjectMapper.Map<List<CallAggregateAttribute>, List<CallAggregateAttributeDto>>(elasticCallAggregateAttributes);
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create call aggregate");
            throw new Exception("Failed to create call aggregate");
        }
    }

    [HttpPut("update-call-aggregate")]
    public async Task<CallAggregateDto> UpdateCallAggregate(CallAggregateRequest item)
    {
        try
        {
            var callAggregate = await _callAggregateRepository.FindAsync(x => x.Id == item.Id);
            if (callAggregate is null) throw new Exception("CallAggregate not found");

            #region Call Aggregate

            var update = ObjectMapper.Map<CallAggregateRequest, CallAggregate>(item);
            
            callAggregate.LastModifierId = _currentUser.Id;
            callAggregate.LastModificationTime = DateTime.Now;
            
            var result = await _callAggregateRepository.UpdateAsync(callAggregate, autoSave: true);
            
            if (item.DynamicAttributeValues is not null && item.DynamicAttributeValues.Any())
            {
                MapDynamicAttributes("call-aggregate", update, item.DynamicAttributeValues);
            }

            #endregion
            
            #region Dynamic entity
            
            List<CallAggregateAttribute> elasticCallAggregateAttributes = new List<CallAggregateAttribute>();
            
            var callAggregateAttributes = await _callAggregateAttributeRepository.GetListAsync(x => x.CallAggregateId == item.Id);
            if (update.CallAggregateAttributes is not null && update.CallAggregateAttributes.Any())
            {
                var callAggregateAttributesToAdd = update.CallAggregateAttributes
                    .Where(a => callAggregateAttributes.All(b => b.DynamicAttributeId != a.DynamicAttributeId))
                    .ToList();
                var callAggregateAttributesToUpdate = callAggregateAttributes
                    .Where(a => update.CallAggregateAttributes.Any(b => b.DynamicAttributeId == a.DynamicAttributeId))
                    .ToList();
                
                if(callAggregateAttributesToAdd.Any())
                {
                    foreach (var callAggregateAttribute in callAggregateAttributesToAdd)
                    {
                        callAggregateAttribute.CallAggregateId = result.Id;
                        callAggregateAttribute.CreatorId = _currentUser.Id;
                        callAggregateAttribute.CreationTime = DateTime.Now;
                    }

                    await _callAggregateAttributeRepository.InsertManyAsync(callAggregateAttributesToAdd, autoSave: true);
                    
                    elasticCallAggregateAttributes.AddRange(callAggregateAttributesToAdd);
                }

                if (callAggregateAttributesToUpdate.Any())
                {
                    foreach (var callAggregateAttribute in callAggregateAttributesToUpdate)
                    {
                        var newCallAggregateAttribute = update.CallAggregateAttributes.FirstOrDefault(b => b.DynamicAttributeId == callAggregateAttribute.DynamicAttributeId);
                        if (newCallAggregateAttribute is not null) callAggregateAttribute.Value = newCallAggregateAttribute.Value;
                        callAggregateAttribute.LastModifierId = _currentUser.Id;
                        callAggregateAttribute.LastModificationTime = DateTime.Now;
                    }

                    await _callAggregateAttributeRepository.UpdateManyAsync(callAggregateAttributesToUpdate, autoSave: true);
                    
                    elasticCallAggregateAttributes.AddRange(callAggregateAttributesToUpdate);
                }
            }
            
            #endregion
            
            await _dynamicEntityElasticService.UpsertCallAggregate(result, elasticCallAggregateAttributes);

            var response = ObjectMapper.Map<CallAggregate, CallAggregateDto>(result);
            if (elasticCallAggregateAttributes.Any())
            {
                response.CallAggregateAttributes = ObjectMapper.Map<List<CallAggregateAttribute>, List<CallAggregateAttributeDto>>(elasticCallAggregateAttributes);
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update call aggregate");
            throw new Exception("Failed to update call aggregate");
        }
    }

    [HttpDelete("delete-call-aggregate/{id}")]
    public async Task DeleteCallAggregate(Guid id)
    {
        try
        {
            var delete = await _callAggregateRepository.FindAsync(x => x.Id == id);
            if(delete is null) throw new Exception("Not found");
            
            await _callAggregateRepository.DeleteAsync(delete, autoSave: true);
            
            var callAggregateAttributes = await _callAggregateAttributeRepository.GetListAsync(x => x.CallAggregateId == id);

            if (callAggregateAttributes.Any())
            {
                await _callAggregateAttributeRepository.DeleteManyAsync(callAggregateAttributes, autoSave: true);
            }
            
            await _dynamicEntityElasticService.DeleteCallAggregate(id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete call aggregate");
            throw new Exception("Failed to delete call aggregate");
        }
    }

    private void MapDynamicAttributes(string entityType, CallAggregate callAggregate, IEnumerable<DynamicAttributeValue>? dynamicAttributeValues)
    {
        var dynamicAttributes = _dynamicEntityAppService.GetDynamicEntity(entityType).Result.DynamicAttributes;
        var callAggregateAttributes = (from item in dynamicAttributeValues
                let dynamicAttribute = dynamicAttributes.FirstOrDefault(x => x.Id == item.DynamicAttributeId)
                                       ?? throw new Exception(
                                           $"Không tồn tại Dynamic Attribute [{item.DynamicAttributeId}]")
                select new CallAggregateAttribute
                {
                    DynamicAttributeId = dynamicAttribute.Id,
                    Value = item.Value,
                    TenantId = callAggregate.TenantId,
                    CustomTenantId = callAggregate.CustomTenantId,
                    SystemName = dynamicAttribute.SystemName,
                    DisplayName = dynamicAttribute.DisplayName,
                    Type = dynamicAttribute.Type,
                })
            .ToList();
        
        callAggregate.CallAggregateAttributes = callAggregateAttributes;
    }
}