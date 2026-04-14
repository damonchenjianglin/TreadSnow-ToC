import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';
import { PetComponent } from './pet.component';

/** 宠物模块路由配置 */
const routes: Routes = [
  { path: '', component: PetComponent, canActivate: [authGuard, permissionGuard] },
];

/**
 * 宠物路由模块
 * 配置宠物页面的路由守卫（认证和权限）
 */
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PetRoutingModule {}
