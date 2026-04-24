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

        /// <summary>
        /// 创建时间起始（可选）
        /// </summary>
        public DateTime? StartCreationTime { get; set; }

        /// <summary>
        /// 创建时间截止（可选）
        /// </summary>
        public DateTime? EndCreationTime { get; set; }

        /// <summary>
        /// 修改时间起始（可选）
        /// </summary>
        public DateTime? StartLastModificationTime { get; set; }

        /// <summary>
        /// 修改时间截止（可选）
        /// </summary>
        public DateTime? EndLastModificationTime { get; set; }
    }
}
