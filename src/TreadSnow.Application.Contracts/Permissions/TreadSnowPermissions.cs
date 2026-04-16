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

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}
