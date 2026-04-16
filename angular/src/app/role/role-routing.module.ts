import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';
import { RoleComponent } from './role.component';

/** 角色-菜单权限模块路由配置 */
const routes: Routes = [
  { path: '', component: RoleComponent, canActivate: [authGuard, permissionGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class RoleRoutingModule {}
