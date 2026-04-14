using System.Threading.Tasks;

namespace TreadSnow.Data;

public interface ITreadSnowDbSchemaMigrator
{
    Task MigrateAsync();
}
