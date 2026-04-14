using Microsoft.Extensions.Localization;
using TreadSnow.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace TreadSnow;

[Dependency(ReplaceServices = true)]
public class TreadSnowBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<TreadSnowResource> _localizer;

    public TreadSnowBrandingProvider(IStringLocalizer<TreadSnowResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
