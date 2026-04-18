using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.Pets
{
    /// <summary>
    /// 创建宠物DTO
    /// </summary>
    public class CreatePetDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid AccountId { get; set; }

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