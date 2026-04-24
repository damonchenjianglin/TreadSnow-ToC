import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';
import { OpportunityComponent } from './opportunity.component';

/** 商机模块路由配置 */
const routes: Routes = [
  { path: '', component: OpportunityComponent, canActivate: [authGuard, permissionGuard] },
];

/**
 * 商机路由模块
 * 配置商机页面的路由守卫（认证和权限）
 */
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OpportunityRoutingModule {}
