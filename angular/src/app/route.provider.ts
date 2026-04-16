import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

/* 自定义路由（在ABP路由注册之前执行） */
export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    const routes = inject(RoutesService);
    routes.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/accounts',
        name: '::Menu:Accounts',
        iconClass: 'fas fa-users',
        layout: eLayoutType.application,
        requiredPolicy: 'TreadSnow.Accounts',
      },
      {
        path: '/pets',
        name: '::Menu:Pets',
        iconClass: 'fas fa-paw',
        layout: eLayoutType.application,
        requiredPolicy: 'TreadSnow.Pets',
      },
      {
        path: '/upload-files',
        name: '::Menu:UploadFiles',
        iconClass: 'fas fa-file-upload',
        layout: eLayoutType.application,
        requiredPolicy: 'TreadSnow.UploadFiles',
      },
      /* 管理菜单下的自定义路由：用户、角色-菜单权限、租户 */
      {
        path: '/users',
        name: '::Menu:Users',
        parentName: 'AbpUiNavigation::Menu:Administration',
        iconClass: 'fas fa-user',
        order: 1,
        layout: eLayoutType.application,
        requiredPolicy: 'AbpIdentity.Users',
      },
      {
        path: '/roles',
        name: '::Menu:RolesPermissions',
        parentName: 'AbpUiNavigation::Menu:Administration',
        iconClass: 'fas fa-user-shield',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'AbpIdentity.Roles',
      },
      {
        path: '/tenants',
        name: '::Menu:Tenants',
        parentName: 'AbpUiNavigation::Menu:Administration',
        iconClass: 'fas fa-building',
        order: 3,
        layout: eLayoutType.application,
        requiredPolicy: 'AbpTenantManagement.Tenants',
      },
    ]);
  }),
];

/* 移除ABP默认注册的菜单项（必须在ABP路由注册之后执行） */
export const APP_ROUTE_PATCH_PROVIDER = [
  provideAppInitializer(() => {
    const routes = inject(RoutesService);
    routes.remove([
      'AbpIdentity::Menu:IdentityManagement',
      'AbpIdentity::Users',
      'AbpIdentity::Roles',
      'AbpTenantManagement::Menu:TenantManagement',
      'AbpTenantManagement::Tenants',
    ]);
  }),
];
