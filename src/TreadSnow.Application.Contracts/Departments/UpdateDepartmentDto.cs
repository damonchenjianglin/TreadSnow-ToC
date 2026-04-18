using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.Departments
{
    /// <summary>
    /// 更新部门DTO
    /// </summary>
    public class UpdateDepartmentDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// 上级部门Id
        /// </summary>
        public Guid? ParentDepartmentId { get; set; }
    }
}
