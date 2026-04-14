import type { EntityDto } from '@abp/ng.core';

//datatable单行类
export interface AuthorDto extends EntityDto<string> {
  name: string;
  birthDate: string; //日期使用string类型
  shortBio?: string;
}

//查询条件
export interface GetAuthorListDto {
  filter?: string;
}

//创建
export interface CreateAuthorDto {
  name: string;
  birthDate: string;
  shortBio?: string;
}

//更新
export interface UpdateAuthorDto {
  name: string;
  birthDate: string;
  shortBio?: string;
}
