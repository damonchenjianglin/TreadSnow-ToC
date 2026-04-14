using TreadSnow.Localization;
using Volo.Abp.Application.Services;

namespace TreadSnow;

/* Inherit your application services from this class.
 */
public abstract class TreadSnowAppService : ApplicationService
{
    protected TreadSnowAppService()
    {
        LocalizationResource = typeof(TreadSnowResource);
    }
}
