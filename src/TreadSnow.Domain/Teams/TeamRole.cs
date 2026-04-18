using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace TreadSnow.Teams
{
    /// <summary>
    /// 团队-角色关系表
    /// </summary>
    public class TeamRole : Entity, IMultiTenant
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 团队Id
        /// </summary>
        public Guid TeamId { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 获取复合主键
        /// </summary>
        /// <returns>复合主键数组</returns>
        public override object[] GetKeys()
        {
            return new object[] { TeamId, RoleId };
        }
    }
}
