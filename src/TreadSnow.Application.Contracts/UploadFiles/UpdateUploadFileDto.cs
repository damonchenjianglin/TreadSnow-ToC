using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.UploadFiles
{
    public class UpdateUploadFileDto
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
    }
}