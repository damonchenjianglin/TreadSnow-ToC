using Volo.Abp.Modularity;

namespace TreadSnow;

[DependsOn(
    typeof(TreadSnowDomainModule),
    typeof(TreadSnowTestBaseModule)
)]
public class TreadSnowDomainTestModule : AbpModule
{

}
