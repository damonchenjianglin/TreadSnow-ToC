import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';
import { UserComponent } from './user.component';

/** 用户管理模块路由配置 */
const routes: Routes = [
  { path: '', component: UserComponent, canActivate: [authGuard, permissionGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class UserRoutingModule {}
