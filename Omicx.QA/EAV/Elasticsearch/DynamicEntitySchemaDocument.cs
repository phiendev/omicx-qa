using Nest;
using Omicx.QA.Elasticsearch.Documents;

namespace Omicx.QA.EAV.Elasticsearch;

[ElasticsearchType(RelationName = "dynamic-entity-schemas", IdProperty = nameof(Id))]
public class DynamicEntitySchemaDocument : ElasticNestedEntity
{
    public Guid Id { get; set; }
    
    public Guid? TenantId { get; set; }
    
    public int? CustomTenantId { get; set; }
    
    public required string EntityType { get; set; }
    
    public required string Label { get; set; }
    
    public required string HashedProperties { get; set; }
    
    [Nested]
    public List<AttributeGroupDocument>? AttributeGroups { get; set; }
}

public class AttributeGroupDocument
{
    public Guid Id { get; set; }
    
    public Guid? TenantId { get; set; }
    
    public int? CustomTenantId { get; set; }
    
    public required string AttributeGroupCode { get; set; }
    
    public string? AttributeGroupName { get; set; }
    
    public Guid? DynamicEntitySchemaId { get; set; }
    
    [Nested]
    public List<DynamicAttributeDocument>? DynamicAttributes { get; set; }
}

public class DynamicAttributeDocument
{
    public Guid Id { get; set; }
    
    public Guid? TenantId { get; set; }

    public int? CustomTenantId { get; set; }
    
    public Guid? DynamicEntitySchemaId { get; set; }
    
    public Guid? AttributeGroupId { get; set; }
    
    public required int Type { get; set; }
    
    public required string SystemName { get; set; }
    
    public required string DisplayName { get; set; }
    
    public string? DesignerOptions { get; set; }
    
    public bool IsActive { get; set; }
    
    public int? Order { get; set; }
}