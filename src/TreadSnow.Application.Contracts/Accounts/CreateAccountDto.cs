using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.Accounts
{
    /// <summary>
    /// 创建会员DTO
    /// </summary>
    public class CreateAccountDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OpenId { get; set; } = string.Empty;

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