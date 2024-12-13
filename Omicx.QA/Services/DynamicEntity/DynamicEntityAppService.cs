using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.MultiTenancy.Customs;
using Omicx.QA.Services.DynamicEntity.Dto;
using Omicx.QA.Services.Elastic;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Omicx.QA.Services.DynamicEntity;

[Route("api/app/dynamic-entity")]
public class DynamicEntityAppService : ApplicationService, IDynamicEntityAppService
{
    private readonly ICurrentCustomTenant _currentCustomTenant;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DynamicEntityAppService> _logger;
    private readonly IRepository<DynamicEntitySchema, Guid> _dynamicEntitySchemaRepository;
    private readonly IRepository<AttributeGroup, Guid> _attributeGroupRepository;
    private readonly IRepository<DynamicAttribute, Guid> _dynamicAttributeRepository;
    private readonly IDynamicEntityElasticService _dynamicEntityElasticService;
    
    public DynamicEntityAppService(
        ICurrentCustomTenant currentCustomTenant,
        ICurrentUser currentUser,
        ILogger<DynamicEntityAppService> logger,
        IRepository<DynamicEntitySchema, Guid> dynamicEntitySchemaRepository,
        IRepository<AttributeGroup, Guid> attributeGroupRepository,
        IRepository<DynamicAttribute, Guid> dynamicAttributeRepository,
        IDynamicEntityElasticService dynamicEntityElasticService
        )
    {
        _currentCustomTenant = currentCustomTenant;
        _currentUser = currentUser;
        _logger = logger;
        _dynamicEntitySchemaRepository = dynamicEntitySchemaRepository;
        _dynamicEntityElasticService = dynamicEntityElasticService;
        _attributeGroupRepository = attributeGroupRepository;
        _dynamicAttributeRepository = dynamicAttributeRepository;
    }

    [HttpPost("sync-schema")]
    public async Task SyncScheme()
    {
        try
        {
            var schemas = await _dynamicEntitySchemaRepository.GetListAsync();
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 5
            };
            await Parallel.ForEachAsync(schemas, parallelOptions, async (schema, cancellationToken) =>
            {
                await _dynamicEntityElasticService.SyncSchema(schema.Id);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync schema");
            throw new Exception("Failed to create schema");
        }
    }

    [HttpPost("create-schema")]
    public async Task<DynamicEntitySchemaDto> CreateSchema(DynamicEntitySchemaDto item)
    {
        try
        {
            var add = ObjectMapper.Map<DynamicEntitySchemaDto, DynamicEntitySchema>(item);
            add.TenantId = _currentCustomTenant.Id;
            add.CustomTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();
            add.CreatorId = _currentUser.Id;
            add.CreationTime = DateTime.Now;
            
            var result = await _dynamicEntitySchemaRepository.InsertAsync(add, autoSave: true);

            await _dynamicEntityElasticService.UpsertSchema(result);
            
            return ObjectMapper.Map<DynamicEntitySchema, DynamicEntitySchemaDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create schema");
            throw new Exception("Failed to create schema");
        }
    }
    
    [HttpPut("update-schema")]
    public async Task<DynamicEntitySchemaDto> UpdateSchema(DynamicEntitySchemaDto item)
    {
        try
        {
            var update = await _dynamicEntitySchemaRepository.FindAsync(x => x.Id == item.Id);
            if(update is null) throw new Exception("Not found");

            update.EntityType = item.EntityType;
            update.Label = item.Label;
            update.HashedProperties = item.HashedProperties;
            update.LastModifierId = _currentUser.Id;
            update.LastModificationTime = DateTime.Now;
            
            var result = await _dynamicEntitySchemaRepository.UpdateAsync(update, autoSave: true);

            await _dynamicEntityElasticService.UpsertSchema(result);
            
            return ObjectMapper.Map<DynamicEntitySchema, DynamicEntitySchemaDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create schema");
            throw new Exception("Failed to create schema");
        }
    }
    
    [HttpDelete("delete-schema")]
    public async Task DeleteSchema(Guid id)
    {
        try
        {
            var delete = await _dynamicEntitySchemaRepository.FindAsync(x => x.Id == id);
            if(delete is null) throw new Exception("Not found");
            
            await _dynamicEntitySchemaRepository.DeleteAsync(delete, autoSave: true);
            
            await _dynamicEntityElasticService.DeleteSchema(id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete todo");
            throw new Exception("Failed to delete todo");
        }
    }

    [HttpPost("create-attribute-group")]
    public async Task<AttributeGroupDto> CreateAttributeGroup(AttributeGroupDto item)
    {
        try
        {
            var add = ObjectMapper.Map<AttributeGroupDto, AttributeGroup>(item);
            add.TenantId = _currentCustomTenant.Id;
            add.CustomTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();
            add.CreatorId = _currentUser.Id;
            add.CreationTime = DateTime.Now;
            
            var result = await _attributeGroupRepository.InsertAsync(add, autoSave: true);

            if (add.DynamicEntitySchemaId is not null)
                await _dynamicEntityElasticService.SyncSchema(result.DynamicEntitySchemaId);
            
            return ObjectMapper.Map<AttributeGroup, AttributeGroupDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create attribute group");
            throw new Exception("Failed to create attribute group");
        }
    }
    
    [HttpPut("update-attribute-group")]
    public async Task<AttributeGroupDto> UpdateAttributeGroup(AttributeGroupDto item)
    {
        try
        {
            var update = await _attributeGroupRepository.FindAsync(x => x.Id == item.Id);
            if(update is null) throw new Exception("Not found");

            update.AttributeGroupCode = item.AttributeGroupCode;
            update.AttributeGroupName = item.AttributeGroupName;
            update.DynamicEntitySchemaId = item.DynamicEntitySchemaId;
            update.LastModifierId = _currentUser.Id;
            update.LastModificationTime = DateTime.Now;
            
            var result = await _attributeGroupRepository.UpdateAsync(update, autoSave: true);

            if (result.DynamicEntitySchemaId is not null)
                await _dynamicEntityElasticService.SyncSchema(result.DynamicEntitySchemaId);
            
            return ObjectMapper.Map<AttributeGroup, AttributeGroupDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create attribute group");
            throw new Exception("Failed to create attribute group");
        }
    }
    
    [HttpDelete("delete-attribute-group")]
    public async Task DeleteAttributeGroup(Guid id)
    {
        try
        {
            var delete = await _attributeGroupRepository.FindAsync(x => x.Id == id);
            if(delete is null) throw new Exception("Not found");
            
            await _attributeGroupRepository.DeleteAsync(delete, autoSave: true);
            
            if(delete.DynamicEntitySchemaId is not null)
                await _dynamicEntityElasticService.SyncSchema(delete.DynamicEntitySchemaId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete attribute group");
            throw new Exception("Failed to delete attribute group");
        }
    }
    
    [HttpPost("create-dynamic-attribute")]
    public async Task<DynamicAttributeDto> CreateDynamicAttribute(DynamicAttributeDto item)
    {
        try
        {
            var add = ObjectMapper.Map<DynamicAttributeDto, DynamicAttribute>(item);
            
            var schema = await _dynamicEntitySchemaRepository.FindAsync(x => x.Id == item.DynamicEntitySchemaId);
            if(schema is not null) add.EntityType = schema.EntityType;

            var attributeGroup = await _attributeGroupRepository.FindAsync(x => x.Id == item.AttributeGroupId);
            if(attributeGroup is not null) add.AttributeGroupCode = attributeGroup.AttributeGroupCode;
            
            add.TenantId = _currentCustomTenant.Id;
            add.CustomTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();
            add.CreatorId = _currentUser.Id;
            add.CreationTime = DateTime.Now;
            
            var result = await _dynamicAttributeRepository.InsertAsync(add, autoSave: true);

            if (add.DynamicEntitySchemaId is not null)
                await _dynamicEntityElasticService.SyncSchema(result.DynamicEntitySchemaId);
            
            return ObjectMapper.Map<DynamicAttribute, DynamicAttributeDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create dynamic attribute");
            throw new Exception("Failed to create dynamic attribute");
        }
    }
    
    [HttpPut("update-dynamic-attribute")]
    public async Task<DynamicAttributeDto> UpdateDynamicAttribute(DynamicAttributeDto item)
    {
        try
        {
            var update = await _dynamicAttributeRepository.FindAsync(x => x.Id == item.Id);
            if(update is null) throw new Exception("Not found");
            
            var schema = await _dynamicEntitySchemaRepository.FindAsync(x => x.Id == item.DynamicEntitySchemaId);
            if(schema is not null) update.EntityType = schema.EntityType;

            var attributeGroup = await _attributeGroupRepository.FindAsync(x => x.Id == item.AttributeGroupId);
            if(attributeGroup is not null) update.AttributeGroupCode = attributeGroup.AttributeGroupCode;
            
            update.DynamicEntitySchemaId = item.DynamicEntitySchemaId;
            update.AttributeGroupId = item.AttributeGroupId;
            update.Type = item.Type;
            update.SystemName = item.SystemName;
            update.DisplayName = item.DisplayName;
            update.DesignerOptions = item.DesignerOptions;
            update.IsActive = item.IsActive;
            update.LastModifierId = _currentUser.Id;
            update.LastModificationTime = DateTime.Now;
            
            var result = await _dynamicAttributeRepository.UpdateAsync(update, autoSave: true);
            
            if (result.DynamicEntitySchemaId is not null)
                await _dynamicEntityElasticService.SyncSchema(result.DynamicEntitySchemaId);
            
            return ObjectMapper.Map<DynamicAttribute, DynamicAttributeDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create dynamic attribute");
            throw new Exception("Failed to create dynamic attribute");
        }
    }

    [HttpDelete("delete-dynamic-attribute")]
    public async Task DeleteDynamicAttribute(Guid id)
    {
        try
        {
            var delete = await _dynamicAttributeRepository.FindAsync(x => x.Id == id);
            if(delete is null) throw new Exception("Not found");

            await _dynamicAttributeRepository.DeleteAsync(delete, autoSave: true);
            
            if(delete.DynamicEntitySchemaId is not null)
                await _dynamicEntityElasticService.SyncSchema(delete.DynamicEntitySchemaId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete dynamic attribute");
            throw new Exception("Failed to delete dynamic attribute");
        }
    }

    [HttpGet("{entityType}/attibute")]
    public async Task<DynamicEntityDto> GetDynamicEntity(string entityType)
    {
        try
        {
            var scheme = await _dynamicEntitySchemaRepository.FindAsync(x => x.EntityType == entityType);
            if(scheme is null) throw new Exception("Entity Type not found.");

            var attributeGroups =
                await _attributeGroupRepository.GetListAsync(x => x.DynamicEntitySchemaId == scheme.Id);
            var dynamicAttributes = await _dynamicAttributeRepository.GetListAsync(x =>
                x.DynamicEntitySchemaId == scheme.Id);

            var schemaMap = ObjectMapper.Map<DynamicEntitySchema, DynamicEntityDto>(scheme);
            if(attributeGroups.Count > 0)
                schemaMap.AttributeGroups = ObjectMapper.Map<List<AttributeGroup>, List<AttributeGroupDto>>(attributeGroups);
            if(dynamicAttributes.Count > 0)
                schemaMap.DynamicAttributes = ObjectMapper.Map<List<DynamicAttribute>, List<DynamicAttributeDto>>(dynamicAttributes);

            return schemaMap;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dynamic schema");
            throw new Exception("Failed to get dynamic schema");
        }
    }

    [HttpGet("{entityType}/{id}/link")]
    public Dictionary<string, string> GetDynamicLink(string entityType, string id)
    {
        try
        {
            switch (entityType)
            {
                case "call-aggregate":
                    return new Dictionary<string, string>
                    {
                        { "detail", $"/call/detail?id={id}" }
                    };
                case "email-receive":
                    return new Dictionary<string, string>
                    {
                        { "detail", $"/email/detail?id={id}" }
                    };
                default:
                    return new Dictionary<string, string>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dynamic link");
            throw new Exception("Failed to get dynamic link");
        }
    }
}