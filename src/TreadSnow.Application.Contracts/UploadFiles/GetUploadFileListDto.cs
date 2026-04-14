using Volo.Abp.Application.Dtos;

namespace TreadSnow.UploadFiles
{
    /// <summary>
    /// 附件查询条件DTO
    /// EntityName和RecordId为可选，不传则查询所有附件
    /// </summary>
    public class GetUploadFileListDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 实体名称（可选，用于子表场景按实体筛选）
        /// </summary>
        public string? EntityName { get; set; }

        /// <summary>
        /// 记录Id（可选，用于子表场景按记录筛选）
        /// </summary>
        public string? RecordId { get; set; }
    }
}