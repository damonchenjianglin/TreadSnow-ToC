import type { EntityDto } from '@abp/ng.core';

/** 商机DTO，用于列表展示和详情查看 */
export interface OpportunityDto extends EntityDto<string> {
  /** 租户ID（多租户隔离） */
  tenantId?: string;
  /** 编号（自增，只读） */
  no: number;
  /** 商机名称 */
  name?: string;
  /** 所属客户ID */
  accountId?: string;
  /** 客户名称（关联查询） */
  accountName?: string;
  /** 描述 */
  description?: string;
  /** 负责人Id */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
  /** 负责人名称 */
  ownerName?: string;
  /** 负责团队名称 */
  ownerTeamName?: string;
  /** 创建人Id */
  creatorId?: string;
  /** 创建时间 */
  creationTime?: string;
  /** 创建人名称 */
  creatorName?: string;
  /** 最后修改人Id */
  lastModifierId?: string;
  /** 最后修改时间 */
  lastModificationTime?: string;
  /** 最后修改人名称 */
  lastModifierName?: string;
  /** 当前用户是否可编辑该记录 */
  canEdit?: boolean;
  /** 当前用户是否可删除该记录 */
  canDelete?: boolean;
}

/** 客户下拉选项DTO（用于商机表单选择客户） */
export interface AccountLookupDto extends EntityDto<string> {
  /** 客户名称 */
  name?: string;
}

/** 用户下拉选项DTO */
export interface UserLookupDto extends EntityDto<string> {
  /** 用户名 */
  name?: string;
}

/** 团队下拉选项DTO */
export interface TeamLookupDto extends EntityDto<string> {
  /** 团队名称 */
  name?: string;
}

/** 创建商机DTO */
export interface CreateOpportunityDto {
  /** 商机名称（必填） */
  name: string;
  /** 所属客户ID（必填） */
  accountId: string;
  /** 描述 */
  description?: string;
  /** 负责人Id（不传则默认当前用户） */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
}

/** 更新商机DTO */
export interface UpdateOpportunityDto {
  /** 商机名称（必填） */
  name: string;
  /** 所属客户ID（必填） */
  accountId: string;
  /** 描述 */
  description?: string;
  /** 负责人Id */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
}
