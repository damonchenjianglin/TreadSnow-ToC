using System;
using System.ComponentModel.DataAnnotations;

namespace TreadSnow.Teams
{
    /// <summary>
    /// 创建团队DTO
    /// </summary>
    public class CreateTeamDto
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        [StringLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// 所属部门Id
        /// </summary>
        public Guid? DepartmentId { get; set; }
    }
}
