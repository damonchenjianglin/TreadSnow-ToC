using Volo.Abp.Modularity;

namespace TreadSnow;

[DependsOn(
    typeof(TreadSnowApplicationModule),
    typeof(TreadSnowDomainTestModule)
)]
public class TreadSnowApplicationTestModule : AbpModule
{

}
