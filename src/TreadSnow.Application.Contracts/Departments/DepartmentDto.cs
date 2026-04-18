using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Departments
{
    /// <summary>
    /// 部门DTO
    /// </summary>
    public class DepartmentDto : EntityDto<Guid>
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
        /// 上级部门Id
        /// </summary>
        public Guid? ParentDepartmentId { get; set; }

        /// <summary>
        /// 上级部门名称
        /// </summary>
        public string? ParentDepartmentName { get; set; }
    }
}
