using Microsoft.AspNetCore.Mvc;
using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.Entities.CallAggregate;
using Omicx.QA.MultiTenancy.Customs;
using Omicx.QA.Services.Call.Dto;
using Omicx.QA.Services.Call.Request;
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
    private readonly IDynamicEntityElasticService _dynamicEntityElasticService;
    private readonly Task<int?> _customTenantId;
    
    public CallAggregateAppService(
        ICurrentCustomTenant currentCustomTenant,
        ICurrentUser currentUser,
        ILogger<CallAggregateAppService> logger,
        IRepository<CallAggregate, Guid> callAggregateRepository,
        IDynamicEntityElasticService dynamicEntityElasticService
        )
    {
        _currentCustomTenant = currentCustomTenant;
        _currentUser = currentUser;
        _logger = logger;
        _callAggregateRepository = callAggregateRepository;
        _dynamicEntityElasticService = dynamicEntityElasticService;
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
            
            var result = await _callAggregateRepository.InsertAsync(add, autoSave: true);
            
            MapTicketDynamicAttributes(add, item.DynamicAttributeValues);

            await _dynamicEntityElasticService.UpsertCallAggregate(result);
            
            return ObjectMapper.Map<CallAggregate, CallAggregateDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create call aggregate");
            throw new Exception("Failed to create call aggregate");
        }
    }
    
    private void MapTicketDynamicAttributes(CallAggregate callAggregate, IEnumerable<DynamicAttributeValue>? dynamicAttributeValues)
    {
        // var dynamicAttributes = _dynamicEntitySchemaService.GetAsync("call-aggregate").Result.DynamicAttributes;
        // var callAggregateAttributes = (from item in dynamicAttributeValues
        //         let dynamicAttribute = dynamicAttributes.FirstOrDefault(x => x.Id == item.DynamicAttributeId)
        //                                ?? throw new Exception(
        //                                    $"Không tồn tại Dynamic Attribute [{item.DynamicAttributeId}]")
        //         select new CallAggregateAttribute
        //         {
        //             DynamicAttributeId = dynamicAttribute.Id,
        //             Value = item.Value,
        //             TenantId = callAggregate.TenantId,
        //             CustomTenantId = callAggregate.CustomTenantId,
        //             CreatorId = _currentUser.Id,
        //             CreationTime = DateTime.Now,
        //         })
        //     .ToList();
        //
        // callAggregate.CallAggregateAttributes = callAggregateAttributes;
    }
}