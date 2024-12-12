using DeviceDetectorNET;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.Entities.CallAggregate;
using Omicx.QA.Enums;
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
    private readonly IMongoCollection<CallAggregateAttribute> _callAggregateAttributeCollection;
    private readonly IDynamicEntityElasticService _dynamicEntityElasticService;
    private readonly IDynamicEntityAppService _dynamicEntityAppService;
    private readonly Task<int?> _customTenantId;
    
    public CallAggregateAppService(
        ICurrentCustomTenant currentCustomTenant,
        ICurrentUser currentUser,
        ILogger<CallAggregateAppService> logger,
        IRepository<CallAggregate, Guid> callAggregateRepository,
        IRepository<CallAggregateAttribute, Guid> callAggregateAttributeRepository,
        IMongoCollection<CallAggregateAttribute> callAggregateAttributeCollection,
        IDynamicEntityElasticService dynamicEntityElasticService,
        IDynamicEntityAppService dynamicEntityAppService
        )
    {
        _currentCustomTenant = currentCustomTenant;
        _currentUser = currentUser;
        _logger = logger;
        _callAggregateRepository = callAggregateRepository;
        _callAggregateAttributeRepository = callAggregateAttributeRepository;
        _callAggregateAttributeCollection = callAggregateAttributeCollection;
        _dynamicEntityElasticService = dynamicEntityElasticService;
        _dynamicEntityAppService = dynamicEntityAppService;
        _customTenantId = _currentCustomTenant.GetCustomTenantIdAsync();
    }

    [HttpPost("create-call-aggregate")]
    public async Task<CallAggregateDto> CreateCallAggregate(CallAggregateRequest item)
    {
        try
        {
            var add = ObjectMapper.Map<CallAggregateRequest, CallAggregate>(item);
            add.TenantId = _currentCustomTenant.Id;
            add.CustomTenantId = await _customTenantId;
            add.CreatorId = _currentUser.Id;
            add.CreationTime = DateTime.Now;
            
            add.Links = _dynamicEntityAppService.GetDynamicLink("call-aggregate", add.CallId);
            
            var result = await _callAggregateRepository.InsertAsync(add, autoSave: true);
            
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
            }

            await _dynamicEntityElasticService.UpsertCallAggregate(result, add.CallAggregateAttributes);
            
            return ObjectMapper.Map<CallAggregate, CallAggregateDto>(result);
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
            var callAggregate = _callAggregateRepository.FindAsync(x => x.Id == item.Id);
            if (callAggregate is null) throw new Exception("CallAggregate not found");
            
            var update = ObjectMapper.Map<CallAggregateRequest, CallAggregate>(item);
            update.LastModifierId = _currentUser.Id;
            update.LastModificationTime = DateTime.Now;
            
            update.Links = _dynamicEntityAppService.GetDynamicLink("call-aggregate", update.CallId);
            
            var result = await _callAggregateRepository.UpdateAsync(update, autoSave: true);
            
            if (item.DynamicAttributeValues is not null && item.DynamicAttributeValues.Any())
            {
                MapDynamicAttributes("call-aggregate", update, item.DynamicAttributeValues);
            }
            
            var callAggregateAttributes = await _callAggregateAttributeRepository.GetListAsync(x => x.CallAggregateId == item.Id);
            if (update.CallAggregateAttributes is not null && update.CallAggregateAttributes.Any())
            {
                var idsToDelete = callAggregateAttributes
                    .Where(a => update.CallAggregateAttributes.All(b => b.DynamicAttributeId != a.DynamicAttributeId))
                    .Select(x => x.Id)
                    .ToList();
                
                if(idsToDelete.Any())
                {
                    await _callAggregateAttributeRepository.DeleteManyAsync(idsToDelete);
                }
            }
            
            var bulkOperations = new List<WriteModel<CallAggregateAttribute>>();
            foreach (var callAggregateAttribute in callAggregateAttributes)
            {
                var existingItem = await _callAggregateAttributeCollection
                    .Find(Builders<CallAggregateAttribute>.Filter.Eq(x => x.Id, callAggregateAttribute.Id))
                    .FirstOrDefaultAsync();
                if (existingItem is null)
                {
                    callAggregateAttribute.CreationTime = DateTime.Now;
                    callAggregateAttribute.CreatorId = _currentUser.Id;
                }
                else
                {
                    callAggregateAttribute.LastModificationTime = DateTime.Now;
                    callAggregateAttribute.LastModifierId = _currentUser.Id;
                }
                bulkOperations.Add(new ReplaceOneModel<CallAggregateAttribute>(
                    Builders<CallAggregateAttribute>.Filter.Eq(x => x.Id, callAggregateAttribute.Id),
                    callAggregateAttribute
                ) { IsUpsert = true });
            }

            if (bulkOperations.Any())
            {
                await _callAggregateAttributeCollection.BulkWriteAsync(bulkOperations);
            }
            
            await _dynamicEntityElasticService.UpsertCallAggregate(result, update.CallAggregateAttributes);
            
            return ObjectMapper.Map<CallAggregate, CallAggregateDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create call aggregate");
            throw new Exception("Failed to create call aggregate");
        }
    }

    [HttpDelete("delete-call-aggregate")]
    public Task DeleteCallAggregate(Guid id)
    {
        throw new NotImplementedException();
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