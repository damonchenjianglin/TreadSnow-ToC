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
    }
}