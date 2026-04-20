using System;
using Volo.Abp.Application.Dtos;

namespace TreadSnow.UploadFiles
{
    public class UploadFileDto : EntityDto<Guid>
    {
        public Guid? TenantId { get; set; }
        public string EntityName { get; set; }
        public string RecordId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }

        public string Url { get; set; }

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