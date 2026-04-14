import { ListService, PagedResultDto } from '@abp/ng.core';
import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgbDateNativeAdapter, NgbDateAdapter } from '@ng-bootstrap/ng-bootstrap';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { AuthorService, AuthorDto, CreateAuthorDto, UpdateAuthorDto } from '../proxy/authors';
@Component({
  standalone: false,
  selector: 'app-author',
  templateUrl: './author.component.html',
  styleUrls: ['./author.component.scss'],
  providers: [ListService, { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
})
export class AuthorComponent implements OnInit {
  author = { items: [], totalCount: 0 } as PagedResultDto<AuthorDto>;
  selectedAuthor = {} as AuthorDto; // declare selectedAuthor
  form!: FormGroup;
  isModalOpen = false;
  authors: AuthorDto[] = [];
  constructor(
    public readonly list: ListService,
    private authorService: AuthorService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService // inject the ConfirmationService
  ) {}

  ngOnInit() {
     const authorStreamCreator = (query: string) => this.authorService.getList(query);

    this.list.hookToQuery(authorStreamCreator).subscribe(response => {
      this.author = response;
    });
  }

  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedAuthor.name || '', Validators.required],
      birthDate: [
        this.selectedAuthor.birthDate ? new Date(this.selectedAuthor.birthDate) : null,
        Validators.required,
      ],
      shortBio: [this.selectedAuthor.shortBio || ''],
    });
  }
  //创建
  createAuthor() {
    this.selectedAuthor = {} as CreateAuthorDto; // reset the selected author
    this.buildForm();
    this.isModalOpen = true;
  }
  //更新
  editAuthor(id: string) {
    this.authorService.get(id).subscribe(author => {
      this.selectedAuthor = author as UpdateAuthorDto;
      this.buildForm();
      this.isModalOpen = true;
    });
  }
  //删除
  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.authorService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  //保存
  save() {
    if (this.form.invalid) {
      return;
    }

    const request = this.selectedAuthor.id
      ? this.authorService.update(this.selectedAuthor.id, this.form.value)
      : this.authorService.create(this.form.value);

    request.subscribe(() => {
      this.isModalOpen = false;
      this.form.reset();
      this.list.get();
    });
  }
}
