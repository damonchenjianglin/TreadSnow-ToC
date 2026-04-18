using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreadSnow.Accounts;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace TreadSnow.Pets
{
    /// <summary>
    /// 宠物
    /// </summary>
    public class Pet : FullAuditedAggregateRoot<Guid>, IMultiTenant
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
        /// 主人
        /// </summary>
        public Guid AccountId { get; set; }
    }
}
