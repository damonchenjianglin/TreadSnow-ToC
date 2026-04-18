using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Teams
{
    /// <summary>
    /// 团队DTO
    /// </summary>
    public class TeamDto : EntityDto<Guid>
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int No { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 所属部门Id
        /// </summary>
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string? DepartmentName { get; set; }
    }
}
