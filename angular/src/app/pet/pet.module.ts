import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';
import { PetRoutingModule } from './pet-routing.module';
import { PetComponent } from './pet.component';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';
import { NzModalModule } from 'ng-zorro-antd/modal';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzMessageModule } from 'ng-zorro-antd/message';
import { NzToolTipModule } from 'ng-zorro-antd/tooltip';

/**
 * 宠物模块
 * 负责宠物功能相关的组件声明和依赖导入
 */
@NgModule({
  declarations: [PetComponent],
  imports: [
    PetRoutingModule,
    SharedModule,
    NzTableModule,
    NzButtonModule,
    NzIconModule,
    NzDropDownModule,
    NzModalModule,
    NzFormModule,
    NzInputModule,
    NzSelectModule,
    NzCardModule,
    NzTagModule,
    NzMenuModule,
    NzGridModule,
    NzMessageModule,
    NzToolTipModule,
    FormsModule,
  ]
})
export class PetModule {}
