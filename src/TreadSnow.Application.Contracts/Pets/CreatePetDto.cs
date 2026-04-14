using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.Pets
{
    public class CreatePetDto
    {
        public Guid? TenantId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid AccountId { get; set; }
    }
}