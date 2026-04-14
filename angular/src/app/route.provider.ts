import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
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
        path: '/books',
        name: '::Menu:Books',
        iconClass: 'fas fa-book',
        layout: eLayoutType.application,
        requiredPolicy: 'TreadSnow.Books',
      },
      {
        path: '/authors',
        name: '::Menu:Authors',
        iconClass: 'fas fa-book',
        layout: eLayoutType.application,
        requiredPolicy: 'TreadSnow.Authors',
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
  ]);
}
