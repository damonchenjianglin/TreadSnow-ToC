import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { authGuard, permissionGuard } from '@abp/ng.core';
import { UploadFileComponent } from './upload-file.component';

/**
 * 附件路由配置
 * 配置附件列表页面的路由，包含认证守卫和权限守卫
 */
const routes: Routes = [
  { path: '', component: UploadFileComponent, canActivate: [authGuard, permissionGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class UploadFileRoutingModule {}
