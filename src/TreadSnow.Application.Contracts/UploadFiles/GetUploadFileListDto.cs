using Volo.Abp.Application.Dtos;

namespace TreadSnow.UploadFiles
{
    public class GetUploadFileListDto : PagedAndSortedResultRequestDto
    {
        public string EntityName { get; set; }
        public string RecordId { get; set; }
    }
}