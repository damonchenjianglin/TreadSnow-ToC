import type { EntityDto } from '@abp/ng.core';

/** 宠物DTO，用于列表展示和详情查看 */
export interface PetDto extends EntityDto<string> {
  /** 租户ID（多租户隔离） */
  tenantId?: string;
  /** 编号（自增，只读） */
  no: number;
  /** 宠物名称 */
  name?: string;
  /** 所属会员ID */
  accountId?: string;
  /** 主人名称（关联查询） */
  accountName?: string;
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

/** 会员下拉选项DTO（用于宠物表单选择主人） */
export interface AccountLookupDto extends EntityDto<string> {
  /** 会员名称 */
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

/** 创建宠物DTO */
export interface CreatePetDto {
  /** 宠物名称（必填） */
  name: string;
  /** 所属会员ID（必填） */
  accountId: string;
  /** 负责人Id（不传则默认当前用户） */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
}

/** 更新宠物DTO */
export interface UpdatePetDto {
  /** 宠物名称（必填） */
  name: string;
  /** 所属会员ID（必填） */
  accountId: string;
  /** 负责人Id */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
}
