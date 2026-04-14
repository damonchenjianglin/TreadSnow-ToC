using TreadSnow.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace TreadSnow.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class TreadSnowController : AbpControllerBase
{
    protected TreadSnowController()
    {
        LocalizationResource = typeof(TreadSnowResource);
    }
}
