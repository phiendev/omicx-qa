namespace Omicx.QA.Engine;

public class MongoDbContext
{
    public MongoDbContext(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
        SharedData = new SharedDataCollection();
    }

    public void SetSharedData<T>(string key, T? value, bool update = false)
    {
        if (value == null) return;
        if (update)
        {
            SharedData.SetOrUpdate(key, value);
            return;
        }

        SharedData.Set(key, value);
    }

    public SharedDataCollection SharedData { get; }
    public CancellationToken CancellationToken { get; }
}