import { RestService } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import { AuthorDto, CreateAuthorDto, UpdateAuthorDto } from './models';

@Injectable({
  providedIn: 'root',
})
export class AuthorService {
  apiName = 'Default';

  //创建
  create = (input: CreateAuthorDto) =>
    this.restService.request<any, AuthorDto>(
      {
        method: 'POST',
        url: '/api/app/author',
        body: input,
      },
      { apiName: this.apiName }
    );
  //删除
  delete = (id: string) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/app/author/${id}`,
      },
      { apiName: this.apiName }
    );
  //获取单个
  get = (id: string) =>
    this.restService.request<any, AuthorDto>(
      {
        method: 'GET',
        url: `/api/app/author/${id}`,
      },
      { apiName: this.apiName }
    );
  //获取列表
  getList = (filter: string) =>
    this.restService.request<any, PagedResultDto<AuthorDto>>(
      {
        method: 'GET',
        url: '/api/app/author',
        params: { filter: filter },
      },
      { apiName: this.apiName }
    );
  //更新
  update = (id: string, input: UpdateAuthorDto) =>
    this.restService.request<any, AuthorDto>(
      {
        method: 'PUT',
        url: `/api/app/author/${id}`,
        body: input,
      },
      { apiName: this.apiName }
    );
  constructor(private restService: RestService) {}
}
