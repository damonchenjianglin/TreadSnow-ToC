using System;
using System.Collections.Generic;

namespace TreadSnow.DataPermissions
{
    /// <summary>
    /// 角色数据权限配置DTO
    /// </summary>
    public class RoleDataPermissionDto
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 权限配置列表
        /// </summary>
        public List<DataPermissionConfigDto> Configs { get; set; } = new List<DataPermissionConfigDto>();
    }
}
