using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;

namespace TreadSnow.EntityFrameworkCore;

public static class TreadSnowEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        TreadSnowGlobalFeatureConfigurator.Configure();
        TreadSnowModuleExtensionConfigurator.Configure();

        OneTimeRunner.Run(() =>
        {
            ObjectExtensionManager.Instance
                .MapEfCoreProperty<IdentityUser, Guid?>(
                    "DepartmentId",
                    (entityBuilder, propertyBuilder) =>
                    {
                        propertyBuilder.HasColumnName("DepartmentId");
                    }
                );
        });
    }
}
