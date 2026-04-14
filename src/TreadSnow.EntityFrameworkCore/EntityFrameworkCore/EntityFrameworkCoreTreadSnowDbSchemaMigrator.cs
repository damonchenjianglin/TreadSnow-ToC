using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TreadSnow.Data;
using Volo.Abp.DependencyInjection;

namespace TreadSnow.EntityFrameworkCore;

public class EntityFrameworkCoreTreadSnowDbSchemaMigrator
    : ITreadSnowDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreTreadSnowDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the TreadSnowDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<TreadSnowDbContext>()
            .Database
            .MigrateAsync();
    }
}
