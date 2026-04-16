import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';
import { RoleRoutingModule } from './role-routing.module';
import { RoleComponent } from './role.component';
import { PermissionManagementModule } from '@abp/ng.permission-management';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';

/** 角色-菜单权限模块 */
@NgModule({
  declarations: [RoleComponent],
  imports: [
    RoleRoutingModule,
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    PermissionManagementModule,
    NzTableModule,
    NzButtonModule,
    NzIconModule,
    NzDropDownModule,
    NzModalModule,
    NzFormModule,
    NzInputModule,
    NzCardModule,
    NzTagModule,
    NzMenuModule,
    NzCheckboxModule,
  ],
})
export class RoleModule {}
