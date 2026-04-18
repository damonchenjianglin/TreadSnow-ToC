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

        var departmentsPermission = myGroup.AddPermission(TreadSnowPermissions.Departments.Default, L("Permission:Departments"));
        departmentsPermission.AddChild(TreadSnowPermissions.Departments.Create, L("Permission:Departments.Create"));
        departmentsPermission.AddChild(TreadSnowPermissions.Departments.Edit, L("Permission:Departments.Edit"));
        departmentsPermission.AddChild(TreadSnowPermissions.Departments.Delete, L("Permission:Departments.Delete"));

        var teamsPermission = myGroup.AddPermission(TreadSnowPermissions.Teams.Default, L("Permission:Teams"));
        teamsPermission.AddChild(TreadSnowPermissions.Teams.Create, L("Permission:Teams.Create"));
        teamsPermission.AddChild(TreadSnowPermissions.Teams.Edit, L("Permission:Teams.Edit"));
        teamsPermission.AddChild(TreadSnowPermissions.Teams.Delete, L("Permission:Teams.Delete"));

        var dataPermissionsPermission = myGroup.AddPermission(TreadSnowPermissions.DataPermissions.Default, L("Permission:DataPermissions"));
        dataPermissionsPermission.AddChild(TreadSnowPermissions.DataPermissions.Manage, L("Permission:DataPermissions.Manage"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TreadSnowResource>(name);
    }
}
