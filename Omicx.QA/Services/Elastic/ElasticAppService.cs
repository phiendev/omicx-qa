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
    private Task<int?> _customTenantId;

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
        _customTenantId = _currentCustomTenant.GetCustomTenantIdAsync();
    }

    [HttpPost("create-indexes-elastic")]
    public async Task<bool> CreateIndexesElastic()
    {
        try
        {
            int? customTenantId = await _customTenantId;
            
            foreach (var indice in _indices)
            {
                string index = $"{customTenantId}_{indice}";
                var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(index);
                if (!indexExistsResponse.Exists)
                {
                    var createIndexResponse = await _elasticClient.Indices.CreateAsync(index, c => c
                        .Settings(s => s
                            .NumberOfShards(1)
                            .NumberOfReplicas(1)
                        )
                        .Map(m => m
                            .AutoMap()
                        )
                    );
                    if (createIndexResponse.IsValid) Logger.LogInformation($"Index '{index}' created successfully.");
                    else
                        Logger.LogError(
                            $"Failed to create index '{index}': {createIndexResponse.ServerError?.Error?.Reason}");
                }
                else Logger.LogInformation($"Index '{index}' already exists.");
            }

            return true;
            //return await _elasticClient.CreateIndexAsync<TodoItemDocument>(await _customTenantId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create index.");
            throw new Exception("Failed to create index.");
        }
    }
}