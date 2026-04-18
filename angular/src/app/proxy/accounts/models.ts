import type { EntityDto } from '@abp/ng.core';

/** 会员DTO，用于列表展示和详情查看 */
export interface AccountDto extends EntityDto<string> {
  /** 租户ID */
  tenantId?: string;
  /** 自增编号 */
  no: number;
  /** 会员名称 */
  name?: string;
  /** 手机号码 */
  phone?: string;
  /** 邮箱 */
  email?: string;
  /** 微信OpenId */
  openId?: string;
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
}

/** 创建会员DTO */
export interface CreateAccountDto {
  /** 会员名称 */
  name: string;
  /** 手机号码 */
  phone: string;
  /** 邮箱 */
  email: string;
  /** 微信OpenId */
  openId: string;
  /** 描述 */
  description?: string;
  /** 负责人Id（不传则默认当前用户） */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
}

/** 更新会员DTO */
export interface UpdateAccountDto {
  /** 会员名称 */
  name: string;
  /** 手机号码 */
  phone: string;
  /** 邮箱 */
  email: string;
  /** 微信OpenId */
  openId: string;
  /** 描述 */
  description?: string;
  /** 负责人Id */
  ownerId?: string;
  /** 负责团队Id */
  ownerTeamId?: string;
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
