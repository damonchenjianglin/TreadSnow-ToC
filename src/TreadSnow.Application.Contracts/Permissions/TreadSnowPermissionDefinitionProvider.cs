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
        var accountsPermission = myGroup.AddPermission(TreadSnowPermissions.Accounts.Default, L("Permission:Accounts"));
        accountsPermission.AddChild(TreadSnowPermissions.Accounts.Create, L("Permission:Accounts.Create"));
        accountsPermission.AddChild(TreadSnowPermissions.Accounts.Edit, L("Permission:Accounts.Edit"));
        accountsPermission.AddChild(TreadSnowPermissions.Accounts.Delete, L("Permission:Accounts.Delete"));

        var petsPermission = myGroup.AddPermission(TreadSnowPermissions.Pets.Default, L("Permission:Pets"));
        petsPermission.AddChild(TreadSnowPermissions.Pets.Create, L("Permission:Pets.Create"));
        petsPermission.AddChild(TreadSnowPermissions.Pets.Edit, L("Permission:Pets.Edit"));
        petsPermission.AddChild(TreadSnowPermissions.Pets.Delete, L("Permission:Pets.Delete"));

        var uploadFilesPermission = myGroup.AddPermission(TreadSnowPermissions.UploadFiles.Default, L("Permission:UploadFiles"));
        uploadFilesPermission.AddChild(TreadSnowPermissions.UploadFiles.Create, L("Permission:UploadFiles.Create"));
        uploadFilesPermission.AddChild(TreadSnowPermissions.UploadFiles.Edit, L("Permission:UploadFiles.Edit"));
        uploadFilesPermission.AddChild(TreadSnowPermissions.UploadFiles.Delete, L("Permission:UploadFiles.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TreadSnowResource>(name);
    }
}
