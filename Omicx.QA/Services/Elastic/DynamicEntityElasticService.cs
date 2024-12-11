using AutoMapper;
using Nest;
using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.Entities.CallAggregate;
using Omicx.QA.MultiTenancy.Customs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Omicx.QA.Services.Elastic;

public class DynamicEntityElasticService : IDynamicEntityElasticService, ITransientDependency
{
    private readonly ILogger<DynamicEntityElasticService> _logger;
    private readonly IMapper _mapper;
    private readonly IElasticClient _elasticClient;
    private readonly IRepository<DynamicEntitySchema, Guid> _dynamicEntitySchemaRepository;
    private readonly IRepository<AttributeGroup, Guid> _attributeGroupRepository;
    private readonly IRepository<DynamicAttribute, Guid> _dynamicAttributeRepository;
    private readonly Task<int?> _customTenantId;

    public DynamicEntityElasticService(
        ICurrentCustomTenant currentCustomTenant,
        ILogger<DynamicEntityElasticService> logger,
        IMapper mapper,
        IElasticClient elasticClient,
        IRepository<DynamicEntitySchema, Guid> dynamicEntitySchemaRepository,
        IRepository<AttributeGroup, Guid> attributeGroupRepository,
        IRepository<DynamicAttribute, Guid> dynamicAttributeRepository
        )
    {
        _logger = logger;
        _mapper = mapper;
        _elasticClient = elasticClient;
        _dynamicEntitySchemaRepository = dynamicEntitySchemaRepository;
        _attributeGroupRepository = attributeGroupRepository;
        _dynamicAttributeRepository = dynamicAttributeRepository;
        _customTenantId = currentCustomTenant.GetCustomTenantIdAsync();
    }
    
    public async Task UpsertSchema(DynamicEntitySchema item)
    {
        if (item.CustomTenantId is null) return;
        var document = _mapper.Map<DynamicEntitySchema, DynamicEntitySchemaDocument>(item);
        document.AfterPropertiesSet();
        var indexName = ElasticsearchExtensions.GetIndexName<DynamicEntitySchemaDocument>(item.CustomTenantId);
        var bulkResponse = await _elasticClient.BulkAsync(x => x
            .Index(indexName)
            .Update<DynamicEntitySchemaDocument>(u => u
                .Id(document.Id)
                .Doc(document)
                .DocAsUpsert(true)
            )
        );
        if (!bulkResponse.IsValid)
        {
            _logger.LogError(bulkResponse.DebugInformation);
        }
    }
    
    public async Task DeleteSchema(Guid id)
    {
        int? tenantId = await _customTenantId;
        if (tenantId is null) return;
        var index = ElasticsearchExtensions.GetIndexName<DynamicEntitySchemaDocument>(tenantId);
        var bulkResponse = await _elasticClient.BulkAsync(x => x.Index(index)
            .Delete<DynamicEntitySchemaDocument>(d => d
                .Index(index)
                .Id(id)
            )
        );
        if (!bulkResponse.IsValid)
        {
            _logger.LogError(bulkResponse.DebugInformation);
        }
    }
    
    public async Task SyncSchema(Guid? dynamicEntitySchemaId)
    {
        int? customTenantId = await _customTenantId;
        if (customTenantId is null) return;
        
        var indexName = ElasticsearchExtensions.GetIndexName<DynamicEntitySchemaDocument>(customTenantId);
        
        var schema = await _dynamicEntitySchemaRepository.FindAsync(x => x.Id == dynamicEntitySchemaId);
        if (schema is null) throw new Exception("Schema Not found");
        var document = _mapper.Map<DynamicEntitySchema, DynamicEntitySchemaDocument>(schema);
        
        var attributeGroups =
            await _attributeGroupRepository.GetListAsync(x => x.DynamicEntitySchemaId == dynamicEntitySchemaId);
        if (attributeGroups.Count > 0)
        {
            var documentAttributeGroups = _mapper.Map<List<AttributeGroup>, List<AttributeGroupDocument>>(attributeGroups);
            document.AttributeGroups = documentAttributeGroups;

            var groupedAttributes = await _dynamicAttributeRepository.GetListAsync(x => x.DynamicEntitySchemaId == dynamicEntitySchemaId);
            foreach (var attributeGroup in document.AttributeGroups)
            {
                var dynamicAttributes = groupedAttributes.Where(g => g.AttributeGroupId == attributeGroup.Id).ToList();
                if (dynamicAttributes.Count > 0)
                {
                    var documentDynamicAttributes = _mapper.Map<List<DynamicAttribute>, List<DynamicAttributeDocument>>(dynamicAttributes);
                    attributeGroup.DynamicAttributes = documentDynamicAttributes;
                }
            }
        }
        
        document.AfterPropertiesSet();
        
        var bulkResponse = await _elasticClient.BulkAsync(b => b
            .Index(indexName)
            .Update<DynamicEntitySchemaDocument>(u => u
                .Id(dynamicEntitySchemaId)
                .Doc(document)
                .DocAsUpsert(true)
            )
        );
        if (!bulkResponse.IsValid)
        {
            _logger.LogError(bulkResponse.DebugInformation);
        }
    }

    public async Task UpsertCallAggregate(CallAggregate item)
    {
        throw new NotImplementedException();
    }
}