import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { AuthorRoutingModule } from './author-routing.module';
import { AuthorComponent } from './author.component';
import { NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap'; // add this line

@NgModule({
  declarations: [AuthorComponent],
  imports: [
    AuthorRoutingModule,
    SharedModule,
    NgbDatepickerModule, 
  ]
})
export class AuthorModule { }