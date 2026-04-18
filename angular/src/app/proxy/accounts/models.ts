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
}
