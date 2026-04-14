using TreadSnow.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace TreadSnow.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(TreadSnowEntityFrameworkCoreModule),
    typeof(TreadSnowApplicationContractsModule)
)]
public class TreadSnowDbMigratorModule : AbpModule
{
}
