using System;

namespace TreadSnow.Teams
{
    /// <summary>
    /// 团队用户DTO
    /// </summary>
    public class TeamUserDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }
    }
}
