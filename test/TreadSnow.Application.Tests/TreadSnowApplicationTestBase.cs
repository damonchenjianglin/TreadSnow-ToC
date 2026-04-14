using Volo.Abp.Modularity;

namespace TreadSnow;

public abstract class TreadSnowApplicationTestBase<TStartupModule> : TreadSnowTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
