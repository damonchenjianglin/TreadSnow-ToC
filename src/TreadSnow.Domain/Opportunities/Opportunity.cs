using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreadSnow.Accounts;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace TreadSnow.Opportunities
{
    /// <summary>
    /// 商机
    /// </summary>
    public class Opportunity : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public int No { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 客户Id（外键）
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 负责人Id
        /// </summary>
        public Guid? OwnerId { get; set; }

        /// <summary>
        /// 负责团队Id
        /// </summary>
        public Guid? OwnerTeamId { get; set; }
    }
}
