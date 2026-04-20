using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.Accounts
{
    /// <summary>
    /// 会员DTO
    /// </summary>
    public class AccountDto : EntityDto<Guid>
    {
        /// <summary>
        /// 租户Id
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public int No { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// OpenId
        /// </summary>
        public string? OpenId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 负责人Id
        /// </summary>
        public Guid? OwnerId { get; set; }

        /// <summary>
        /// 负责团队Id
        /// </summary>
        public Guid? OwnerTeamId { get; set; }

        /// <summary>
        /// 负责人名称（关联查询）
        /// </summary>
        public string? OwnerName { get; set; }

        /// <summary>
        /// 负责团队名称（关联查询）
        /// </summary>
        public string? OwnerTeamName { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        public Guid? CreatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 创建人名称（关联查询）
        /// </summary>
        public string? CreatorName { get; set; }

        /// <summary>
        /// 最后修改人Id
        /// </summary>
        public Guid? LastModifierId { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 最后修改人名称（关联查询）
        /// </summary>
        public string? LastModifierName { get; set; }

        /// <summary>
        /// 当前用户是否可编辑该记录（数据权限判断）
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// 当前用户是否可删除该记录（数据权限判断）
        /// </summary>
        public bool CanDelete { get; set; }
    }
}