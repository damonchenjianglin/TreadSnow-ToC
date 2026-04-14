using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.Pets
{
    public class UpdatePetDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid AccountId { get; set; }
    }
}