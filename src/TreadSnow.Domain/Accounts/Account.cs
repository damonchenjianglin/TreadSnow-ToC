using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace TreadSnow.Accounts
{
    /// <summary>
    /// 会员
    /// </summary>
    public class Account : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 编号（自增列）
        /// </summary>
        public int? No { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// OpenId
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}
