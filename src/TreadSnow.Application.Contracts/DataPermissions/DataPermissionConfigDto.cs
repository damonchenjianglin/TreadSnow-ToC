namespace TreadSnow.DataPermissions
{
    /// <summary>
    /// 单个实体的数据权限配置DTO
    /// </summary>
    public class DataPermissionConfigDto
    {
        /// <summary>
        /// 实体名称（如account、pet、uploadFile）
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// 读权限等级（0-4）
        /// </summary>
        public int ReadLevel { get; set; }

        /// <summary>
        /// 写权限等级（0-4）
        /// </summary>
        public int WriteLevel { get; set; }

        /// <summary>
        /// 删除权限等级（0-4）
        /// </summary>
        public int DeleteLevel { get; set; }
    }
}
