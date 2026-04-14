using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.Accounts
{
    public class UpdateAccountDto
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
    }
}