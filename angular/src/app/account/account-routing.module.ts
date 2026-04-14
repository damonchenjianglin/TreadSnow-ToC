import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';
import { AccountComponent } from './account.component';

/** 会员模块路由配置 */
const routes: Routes = [
  { path: '', component: AccountComponent, canActivate: [authGuard, permissionGuard] },
];

/**
 * 会员路由模块
 * 配置会员管理页面的路由和守卫
 */
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AccountRoutingModule {}
