namespace Omicx.QA.Elasticsearch.Configurations;

public interface IElasticsearchConfiguration
{
    string ConnectionStrings { get; set; }
    string Username { get; set; }
    string Password { get; set; }
    string DefaultIndex { get; set; }
}