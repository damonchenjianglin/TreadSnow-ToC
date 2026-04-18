using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.UploadFiles
{
    /// <summary>
    /// 创建附件DTO
    /// </summary>
    public class CreateUploadFileDto
    {
        [Required]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public string RecordId { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Path { get; set; } = string.Empty;

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