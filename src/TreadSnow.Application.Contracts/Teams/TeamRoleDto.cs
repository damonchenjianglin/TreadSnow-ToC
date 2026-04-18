using System;

namespace TreadSnow.Teams
{
    /// <summary>
    /// 团队角色DTO
    /// </summary>
    public class TeamRoleDto
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string? RoleName { get; set; }
    }
}
