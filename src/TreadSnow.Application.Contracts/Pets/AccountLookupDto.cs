using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Pets
{
    /// <summary>
    /// 会员下拉选项DTO（用于宠物表单选择主人）
    /// </summary>
    public class AccountLookupDto : EntityDto<Guid>
    {
        /// <summary>
        /// 会员名称
        /// </summary>
        public string Name { get; set; }
    }
}
