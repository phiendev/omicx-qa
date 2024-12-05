using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Omicx.QA.Elasticsearch.Configurations;
using Volo.Abp.Modularity;

namespace Omicx.QA.Elasticsearch;

public class QAElasticsearchModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var elasticsearchConfiguration = new ElasticsearchConfiguration();
        configuration.GetSection("Elasticsearch").Bind(elasticsearchConfiguration);
        
        context.Services.AddSingleton<IElasticsearchConfiguration>(elasticsearchConfiguration);
        
        context.Services.AddTransient<IElasticsearchManager, ElasticsearchManager>();
    }
}