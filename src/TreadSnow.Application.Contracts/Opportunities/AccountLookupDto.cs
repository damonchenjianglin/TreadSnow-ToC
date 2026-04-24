using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Opportunities
{
    /// <summary>
    /// 客户下拉选项DTO（用于商机表单选择客户）
    /// </summary>
    public class AccountLookupDto : EntityDto<Guid>
    {
        /// <summary>
        /// 客户名称
        /// </summary>
        public string Name { get; set; }
    }
}
