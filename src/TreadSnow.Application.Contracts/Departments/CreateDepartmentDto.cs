using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.Departments
{
    /// <summary>
    /// 创建部门DTO
    /// </summary>
    public class CreateDepartmentDto
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
