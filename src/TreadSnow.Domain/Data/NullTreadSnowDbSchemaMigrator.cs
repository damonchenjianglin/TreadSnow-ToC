using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TreadSnow.Data;

/* This is used if database provider does't define
 * ITreadSnowDbSchemaMigrator implementation.
 */
public class NullTreadSnowDbSchemaMigrator : ITreadSnowDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
