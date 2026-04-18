using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Lookups
{
    /// <summary>
    /// 用户下拉选项DTO（用于负责人选择器）
    /// </summary>
    public class UserLookupDto : EntityDto<Guid>
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { get; set; }
    }
}
