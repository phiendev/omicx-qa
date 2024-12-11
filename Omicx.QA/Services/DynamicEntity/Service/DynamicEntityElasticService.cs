using AutoMapper;
using Nest;
using Omicx.QA.EAV.DynamicEntity;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.MultiTenancy.Customs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Omicx.QA.Services.DynamicEntity.Service;

public class DynamicEntityElasticService : IDynamicEntityElasticService, ITransientDependency
{
    private readonly ICurrentCustomTenant _currentCustomTenant;
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
        _currentCustomTenant = currentCustomTenant;
        _logger = logger;
        _mapper = mapper;
        _elasticClient = elasticClient;
        _dynamicEntitySchemaRepository = dynamicEntitySchemaRepository;
        _attributeGroupRepository = attributeGroupRepository;
        _dynamicAttributeRepository = dynamicAttributeRepository;
        _customTenantId = _currentCustomTenant.GetCustomTenantIdAsync();
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

    public async Task UpsertAttributeGroup(Guid? dynamicEntitySchemaId)
    {
        int? customTenantId = await _customTenantId;
        if (customTenantId is null) return;
        
        var schema = await _dynamicEntitySchemaRepository.FindAsync(x => x.Id == dynamicEntitySchemaId);
        if (schema is null) throw new Exception("Not found");
        
        var attributeGroups = await _attributeGroupRepository.GetListAsync(x => x.DynamicEntitySchemaId == dynamicEntitySchemaId);
        var document = _mapper.Map<DynamicEntitySchema, DynamicEntitySchemaDocument>(schema);
        var documentAttributeGroups = _mapper.Map<List<AttributeGroup>, List<AttributeGroupDocument>>(attributeGroups);
        
        var indexName = ElasticsearchExtensions.GetIndexName<DynamicEntitySchemaDocument>(customTenantId);

        document.AttributeGroups = documentAttributeGroups;
        
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
    
    public async Task DeleteAttributeGroup(Guid? dynamicEntitySchemaId, Guid id)
    {
        if (await _customTenantId is null) return;
        var schema = await _dynamicEntitySchemaRepository.FindAsync(x => x.Id == dynamicEntitySchemaId);
        if (schema is null) throw new Exception("Not found");
        var attributeGroups = await _attributeGroupRepository.GetListAsync(x => x.DynamicEntitySchemaId == dynamicEntitySchemaId);
        var document = _mapper.Map<DynamicEntitySchema, DynamicEntitySchemaDocument>(schema);
        var documentAttributeGroups = _mapper.Map<List<AttributeGroup>, List<AttributeGroupDocument>>(attributeGroups);
        var indexName = ElasticsearchExtensions.GetIndexName<DynamicEntitySchemaDocument>(await _customTenantId);

        document.AttributeGroups = documentAttributeGroups;
        if (document.AttributeGroups is null) document.AttributeGroups = new List<AttributeGroupDocument>();
        var existingGroup = document.AttributeGroups.FirstOrDefault(g => g.Id == id);
        if (existingGroup is not null) document.AttributeGroups.Remove(existingGroup);
        
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

    public async Task UpsertDynamicAttribute(Guid? dynamicEntitySchemaId, Guid? attributeGroupId)
    {
        int? customTenantId = await _customTenantId;
        if (customTenantId is null) return;
        
        var schema = await _dynamicEntitySchemaRepository.FindAsync(x => x.Id == dynamicEntitySchemaId);
        if (schema is null) throw new Exception("Schema Not found");
        
        var attributeGroups =
            await _attributeGroupRepository.GetListAsync(x => x.DynamicEntitySchemaId == dynamicEntitySchemaId);
        if (attributeGroups.Count == 0) throw new Exception("Attribute Group Not found");
        
        var dynamicAttributes = await _dynamicAttributeRepository.GetListAsync(x =>
            x.DynamicEntitySchemaId == dynamicEntitySchemaId && x.AttributeGroupId == attributeGroupId);
        
        var document = _mapper.Map<DynamicEntitySchema, DynamicEntitySchemaDocument>(schema);
        var documentAttributeGroups = _mapper.Map<List<AttributeGroup>, List<AttributeGroupDocument>>(attributeGroups);
        var documentDynamicAttributes = _mapper.Map<List<DynamicAttribute>, List<DynamicAttributeDocument>>(dynamicAttributes);
        
        var indexName = ElasticsearchExtensions.GetIndexName<DynamicEntitySchemaDocument>(customTenantId);

        document.AttributeGroups = documentAttributeGroups;
        var documentAttributeGroup = document.AttributeGroups.FirstOrDefault(x => x.Id == attributeGroupId);
        if (documentAttributeGroup is null) throw new Exception("Attribute Group Not found");

        documentAttributeGroup.DynamicAttributes = documentDynamicAttributes;
        
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

    public async Task DeleteDynamicAttribute(Guid? dynamicEntitySchemaId, Guid? attributeGroupId, Guid id)
    {
        
    }
}