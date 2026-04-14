using Volo.Abp.Modularity;

namespace TreadSnow;

/* Inherit from this class for your domain layer tests. */
public abstract class TreadSnowDomainTestBase<TStartupModule> : TreadSnowTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
