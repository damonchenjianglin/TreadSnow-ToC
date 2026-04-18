using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Pets
{
    /// <summary>
    /// 宠物查询条件DTO
    /// </summary>
    public class GetPetListDto : PagedAndSortedResultRequestDto
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
