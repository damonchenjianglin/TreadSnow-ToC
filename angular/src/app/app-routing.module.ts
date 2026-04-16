import { authGuard, permissionGuard } from '@abp/ng.core';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadChildren: () => import('./home/home.module').then(m => m.HomeModule),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(m => m.AccountModule.forLazy()),
  },
  { path: 'users', loadChildren: () => import('./user/user.module').then(m => m.UserModule) },
  { path: 'roles', loadChildren: () => import('./role/role.module').then(m => m.RoleModule) },
  { path: 'tenants', loadChildren: () => import('./tenant/tenant.module').then(m => m.TenantModule) },
  {
    path: 'setting-management',
    loadChildren: () =>
      import('@abp/ng.setting-management').then(m => m.SettingManagementModule.forLazy()),
  },
  { path: 'accounts', loadChildren: () => import('./account/account.module').then(m => m.AccountModule) },
  { path: 'pets', loadChildren: () => import('./pet/pet.module').then(m => m.PetModule) },
  { path: 'upload-files', loadChildren: () => import('./upload-file/upload-file.module').then(m => m.UploadFileModule) },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {})],
  exports: [RouterModule],
})
export class AppRoutingModule {}
