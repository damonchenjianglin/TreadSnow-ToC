import type { EntityDto } from '@abp/ng.core';

/**
 * 附件DTO
 * 用于附件列表展示和详情查看
 */
export interface UploadFileDto extends EntityDto<string> {
  /** 租户ID */
  tenantId?: string;
  /** 实体名称 */
  entityName?: string;
  /** 记录ID */
  recordId?: string;
  /** 文件名称 */
  name?: string;
  /** 文件类型 */
  type?: string;
  /** 文件路径 */
  path?: string;
  /** 文件访问URL（只读，后端计算） */
  url?: string;
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

/**
 * 新建附件DTO
 * 用于创建新附件时提交的数据
 */
export interface CreateUploadFileDto {
  /** 实体名称 */
  entityName: string;
  /** 记录ID */
  recordId: string;
  /** 文件名称 */
  name: string;
  /** 文件类型 */
  type: string;
  /** 文件路径 */
  path: string;
  /** 负责人Id（不传则默认当前用户） */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
}

/**
 * 更新附件DTO
 * 用于编辑附件时提交的数据
 */
export interface UpdateUploadFileDto {
  /** 实体名称 */
  entityName: string;
  /** 记录ID */
  recordId: string;
  /** 文件名称 */
  name: string;
  /** 文件类型 */
  type: string;
  /** 文件路径 */
  path: string;
  /** 负责人Id */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
}
