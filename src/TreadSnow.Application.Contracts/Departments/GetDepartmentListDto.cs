using Volo.Abp.Application.Dtos;

namespace TreadSnow.Departments
{
    /// <summary>
    /// 部门列表查询条件DTO
    /// </summary>
    public class GetDepartmentListDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 名称模糊筛选
        /// </summary>
        public string? Name { get; set; }
    }
}
