using Microsoft.Extensions.Localization;
using Omicx.QA.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Omicx.QA;

[Dependency(ReplaceServices = true)]
public class QABrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<QAResource> _localizer;

    public QABrandingProvider(IStringLocalizer<QAResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}