using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.Opportunities
{
    /// <summary>
    /// 创建商机DTO
    /// </summary>
    public class CreateOpportunityDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 客户Id（必填）
        /// </summary>
        [Required]
        public Guid AccountId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 负责人Id（不传则默认当前用户）
        /// </summary>
        public Guid? OwnerId { get; set; }

        /// <summary>
        /// 负责团队Id
        /// </summary>
        public Guid? OwnerTeamId { get; set; }
    }
}
