namespace Omicx.QA.Elasticsearch.Configurations;

public class ElasticsearchConfiguration : IElasticsearchConfiguration
{
    public string ConnectionStrings { get; set; } = "http://localhost:9200";
    public string Username { get; set; } = "elastic";
    public string Password { get; set; } = string.Empty;
    public string DefaultIndex { get; set; } = "defaultIndex";
}