import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';
import { TenantRoutingModule } from './tenant-routing.module';
import { TenantComponent } from './tenant.component';
import { FeatureManagementModule } from '@abp/ng.feature-management';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzMenuModule } from 'ng-zorro-antd/menu';

/** 租户管理模块 */
@NgModule({
  declarations: [TenantComponent],
  imports: [
    TenantRoutingModule,
    SharedModule,
    FormsModule,
    ReactiveFormsModule,
    FeatureManagementModule,
    NzTableModule,
    NzButtonModule,
    NzIconModule,
    NzDropDownModule,
    NzModalModule,
    NzFormModule,
    NzInputModule,
    NzCardModule,
    NzMenuModule,
  ],
})
export class TenantModule {}
