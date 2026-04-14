using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.UploadFiles
{
    public class UploadFileDto : EntityDto<Guid>
    {
        public Guid? TenantId { get; set; }
        public string EntityName { get; set; }
        public string RecordId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }

        public string Url { get; set; }
    }
}