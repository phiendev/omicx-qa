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
    private readonly IElasticClient _elasticClient;
    private string[] _indices;
    private Task<int?> _customTenantId;

    public ElasticAppService(
        ICurrentCustomTenant currentCustomTenant,
        IConfiguration configuration,
        IElasticClient elasticClient
    )
    {
        _elasticClient = elasticClient;
        _indices = configuration.GetSection("Elasticsearch:Indices")?.Get<string[]>()
                   ?? Array.Empty<string>();
        _customTenantId = currentCustomTenant.GetCustomTenantIdAsync();
    }

    [HttpPost("create-indexes-elastic")]
    public async Task CreateIndexesElastic()
    {
        try
        {
            int? customTenantId = await _customTenantId;
            if(customTenantId is not null)
            {
                await _elasticClient.CreateIndexAsync<TodoItemDocument>(await _customTenantId);
                await _elasticClient.CreateIndexAsync<CallAggregateDocument>(await _customTenantId);
                await _elasticClient.CreateIndexAsync<EmailReceiveDocument>(await _customTenantId);
                await _elasticClient.CreateIndexAsync<DynamicEntitySchemaDocument>(await _customTenantId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create index.");
            throw new Exception("Failed to create index.");
        }
    }
}