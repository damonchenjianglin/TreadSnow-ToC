using TreadSnow.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace TreadSnow.Permissions;

public class TreadSnowPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(TreadSnowPermissions.GroupName);

        var booksPermission = myGroup.AddPermission(TreadSnowPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(TreadSnowPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(TreadSnowPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(TreadSnowPermissions.Books.Delete, L("Permission:Books.Delete"));

        var authorsPermission = myGroup.AddPermission(TreadSnowPermissions.Authors.Default, L("Permission:Authors"));
        authorsPermission.AddChild(TreadSnowPermissions.Authors.Create, L("Permission:Authors.Create"));
        authorsPermission.AddChild(TreadSnowPermissions.Authors.Edit, L("Permission:Authors.Edit"));
        authorsPermission.AddChild(TreadSnowPermissions.Authors.Delete, L("Permission:Authors.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(TreadSnowPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TreadSnowResource>(name);
    }
}
