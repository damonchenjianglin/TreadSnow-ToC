using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Lookups
{
    /// <summary>
    /// 团队下拉选项DTO（用于负责团队选择器）
    /// </summary>
    public class TeamLookupDto : EntityDto<Guid>
    {
        /// <summary>
        /// 团队名称
        /// </summary>
        public string Name { get; set; }
    }
}
