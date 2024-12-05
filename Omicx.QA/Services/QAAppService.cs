using Volo.Abp.Application.Services;
using Omicx.QA.Localization;

namespace Omicx.QA.Services;

/* Inherit your application services from this class. */
public abstract class QAAppService : ApplicationService
{
    protected QAAppService()
    {
        LocalizationResource = typeof(QAResource);
    }
}