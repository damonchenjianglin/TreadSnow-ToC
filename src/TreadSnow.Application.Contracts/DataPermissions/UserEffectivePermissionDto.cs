using System.Collections.Generic;

namespace TreadSnow.DataPermissions
{
    /// <summary>
    /// 用户合并后的有效数据权限DTO
    /// </summary>
    public class UserEffectivePermissionDto
    {
        /// <summary>
        /// 权限配置列表（多角色取最大值合并后的结果）
        /// </summary>
        public List<DataPermissionConfigDto> Configs { get; set; } = new List<DataPermissionConfigDto>();
    }
}
