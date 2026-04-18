namespace TreadSnow.Permissions;

public static class TreadSnowPermissions
{
    public const string GroupName = "TreadSnow";


    /// <summary>
    /// 会员权限
    /// </summary>
    public static class Accounts
    {
        public const string Default = GroupName + ".Accounts";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>
    /// 宠物权限
    /// </summary>
    public static class Pets
    {
        public const string Default = GroupName + ".Pets";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>
    /// 附件权限
    /// </summary>
    public static class UploadFiles
    {
        public const string Default = GroupName + ".UploadFiles";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>
    /// 部门权限
    /// </summary>
    public static class Departments
    {
        public const string Default = GroupName + ".Departments";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>
    /// 团队权限
    /// </summary>
    public static class Teams
    {
        public const string Default = GroupName + ".Teams";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    /// <summary>
    /// 数据权限配置权限
    /// </summary>
    public static class DataPermissions
    {
        public const string Default = GroupName + ".DataPermissions";
        public const string Manage = Default + ".Manage";
    }
}
