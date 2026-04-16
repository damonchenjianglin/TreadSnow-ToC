import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';
import { TenantComponent } from './tenant.component';

/** 租户模块路由配置 */
const routes: Routes = [
  { path: '', component: TenantComponent, canActivate: [authGuard, permissionGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class TenantRoutingModule {}
