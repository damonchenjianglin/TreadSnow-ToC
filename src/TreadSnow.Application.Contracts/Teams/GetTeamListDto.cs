using Volo.Abp.Application.Dtos;

namespace TreadSnow.Teams
{
    /// <summary>
    /// 团队列表查询条件DTO
    /// </summary>
    public class GetTeamListDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 名称模糊筛选
        /// </summary>
        public string? Name { get; set; }
    }
}
