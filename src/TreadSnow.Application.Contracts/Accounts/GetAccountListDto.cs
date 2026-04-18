using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Accounts
{
    /// <summary>
    /// 会员查询条件DTO
    /// </summary>
    public class GetAccountListDto : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 名称模糊搜索（可选）
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 按负责人筛选（可选）
        /// </summary>
        public Guid? OwnerId { get; set; }
    }
}
