namespace Omicx.QA.Services.Elastic;

public interface IElasticAppService
{
    Task<bool> CreateIndexesElastic();
}