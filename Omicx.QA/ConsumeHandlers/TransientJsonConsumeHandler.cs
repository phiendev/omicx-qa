using CrmCloud.Kafka;
using Volo.Abp.DependencyInjection;

namespace Omicx.QA.ConsumeHandlers;

public abstract class TransientJsonConsumeHandler<T> : AbstractJsonConsumeHandler<T>, ITransientDependency
    where T : class, new()
{
}