using Microsoft.AspNetCore.Mvc;
using Nest;
using Omicx.QA.EAV.Elasticsearch;
using Omicx.QA.Elasticsearch.Extensions;
using Omicx.QA.MultiTenancy.Customs;
using Volo.Abp.Application.Services;

namespace Omicx.QA.Services.Elastic;

[Route("api/app/elastic")]
public class ElasticAppService : ApplicationService, IElasticAppService
{
    private readonly ICurrentCustomTenant _currentCustomTenant;
    private readonly IElasticClient _elasticClient;
    private string[] _indices;

    public ElasticAppService(
        ICurrentCustomTenant currentCustomTenant,
        IConfiguration configuration,
        IElasticClient elasticClient
    )
    {
        _currentCustomTenant = currentCustomTenant;
        _elasticClient = elasticClient;
        _indices = configuration.GetSection("Elasticsearch:Indices")?.Get<string[]>()
                   ?? Array.Empty<string>();
    }

    [HttpPost("create-indexes-elastic")]
    public async Task CreateIndexesElastic()
    {
        try
        {
            int? customTenantId = await _currentCustomTenant.GetCustomTenantIdAsync();
            if(customTenantId is not null)
            {
                await _elasticClient.CreateIndexAsync<TodoItemDocument>(customTenantId);
                await _elasticClient.CreateIndexAsync<CallAggregateDocument>(customTenantId);
                await _elasticClient.CreateIndexAsync<EmailReceiveDocument>(customTenantId);
                await _elasticClient.CreateIndexAsync<DynamicEntitySchemaDocument>(customTenantId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create index.");
            throw new Exception("Failed to create index.");
        }
    }
}