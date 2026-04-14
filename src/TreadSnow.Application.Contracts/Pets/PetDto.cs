using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Pets
{
    /// <summary>
    /// 宠物DTO
    /// </summary>
    public class PetDto : EntityDto<Guid>
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
        /// 主人Id（外键到会员表）
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// 主人名称（关联查询）
        /// </summary>
        public string? AccountName { get; set; }
    }
}