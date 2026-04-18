using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace TreadSnow.DataPermissions
{
    /// <summary>
    /// 角色数据权限配置
    /// </summary>
    public class RoleDataPermission : Entity<Guid>, IMultiTenant
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 权限配置JSON
        /// </summary>
        public string ConfigJson { get; set; }
    }
}
