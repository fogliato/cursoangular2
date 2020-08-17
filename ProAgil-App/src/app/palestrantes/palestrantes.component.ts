import { Component, OnInit } from '@angular/core';
import { Palestrante } from '../models/Palestrante';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { BsLocaleService } from 'ngx-bootstrap/datepicker';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { PalestranteService } from '../services/palestrante.service';
import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';
const Editor = ClassicEditor.Editor;
@Component({
  selector: 'app-palestrantes',
  templateUrl: './palestrantes.component.html',
  styleUrls: ['./palestrantes.component.css'],
})
export class PalestrantesComponent implements OnInit {
  title = 'Palestrantes';
  palestrantesFiltrados: Palestrante[];
  palestrantes: Palestrante[];
  palestrante: Palestrante;
  imagemLargura = 50;
  imagemMargem = 2;
  mostrarImagem = false;
  editMode = false;
  filtroList: string;
  bodyDeletarPalestrante: string;
  imagemUrl = 'assets/img/upload.png';
  modalRef: BsModalRef;
  registerForm: FormGroup;
  file: File;
  fileNameToUpload: string;
  dataImagem: string;
  public miniCurriculoEditor = ClassicEditor;

  constructor(
    private palestranteService: PalestranteService,
    private modalService: BsModalService,
    private fb: FormBuilder,
    private localeService: BsLocaleService,
    private toastr: ToastrService,
    private router: Router
  ) {
    this.localeService.use('pt-br');
  }

  ngOnInit() {
    this.validation();
    this.getPalestrantes();
  }

  get filtroLista(): string {
    return this.filtroList;
  }

  set filtroLista(value: string) {
    this.filtroList = value;
    this.palestrantesFiltrados = this.filtroLista
      ? this.filtrarPalestrantes(this.filtroLista)
      : this.palestrantes;
  }

  openModal(template: any) {
    this.registerForm.reset();
    template.show();
  }

  filtrarPalestrantes(filtrarPor: string): Palestrante[] {
    filtrarPor = filtrarPor.toLocaleLowerCase();
    return this.palestrantes.filter(
      (palestrante) =>
        palestrante.nome.toLocaleLowerCase().indexOf(filtrarPor) !== -1 ||
        palestrante.email.toLocaleLowerCase().indexOf(filtrarPor) !== -1
    );
  }

  alternarImagem() {
    this.mostrarImagem = !this.mostrarImagem;
  }

  validation() {
    this.registerForm = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(4)]],
      imagemUrl: ['', Validators.required],
      telefone: ['', Validators.required],
      miniCurriculo: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
    });
  }

  cancelar(template: any) {
    template.hide();
  }

  salvarAlteracao(template: any) {
    if (this.registerForm.valid) {
      if (!this.editMode) {
        this.palestrante = Object.assign({}, this.registerForm.value);
        this.uploadAndAdjustFileName();
        this.palestranteService.postPalestrante(this.palestrante).subscribe(
          (novo: Palestrante) => {
            template.hide();
            this.getPalestrantes();
            this.toastr.success(
              'Novo palestrante salvo com sucesso',
              'Sucesso!'
            );
          },
          // tslint:disable-next-line: no-shadowed-variable
          (error) => {
            this.toastr.error(
              `Falha ao salvar o novo registro na base de dados.Mensagem: ${error}`,
              'Erro'
            );
          }
        );
      } else {
        this.palestrante = Object.assign(
          { id: this.palestrante.id },
          this.registerForm.value
        );
        this.uploadAndAdjustFileName();
        this.palestranteService.putPalestrante(this.palestrante).subscribe(
          (novo: Palestrante) => {
            template.hide();
            this.getPalestrantes();
            this.toastr.success('Edição gravada com sucesso', 'Sucesso!');
          },
          // tslint:disable-next-line: no-shadowed-variable
          (error) => {
            this.toastr.error(
              `Falha ao editar o registro na base de dados. Mensagem: ${error}`,
              'Erro'
            );
          }
        );
      }
    } else {
      this.toastr.error(`Preencha o formulário corretamente.`, 'Erro');
    }
  }

  private uploadAndAdjustFileName() {
    if (
      this.registerForm.get('imagemUrl').value !== '' &&
      this.registerForm.get('imagemUrl').value !== 'assets/img/upload.png'
    ) {
      this.palestranteService
        .postUpload(this.file, this.fileNameToUpload)
        .subscribe(() => {
          this.dataImagem = new Date().getMilliseconds().toString();
          this.imagemUrl = `http://localhost:5000/Resources/Images/${this.palestrante.imagemUrl}?dtimg=${this.dataImagem}`;
        });
    }
  }

  excluirPalestrante(palestrante: Palestrante, template: any) {
    this.openModal(template);
    this.palestrante = palestrante;
    this.bodyDeletarPalestrante = `Tem certeza que deseja excluir o Palestrante: ${palestrante.nome}, Código: ${palestrante.id}`;
  }

  confirmeDelete(modal: any) {
    this.palestranteService.deletePalestrante(this.palestrante.id).subscribe(
      () => {
        modal.hide();
        this.toastr.success(
          'O Palestrante foi excluído com sucesso.',
          'Sucesso!'
        );
        this.getPalestrantes();
      },
      // tslint:disable-next-line: no-shadowed-variable
      (error) => {
        this.toastr.error('Falha ao excluir registro.', 'Erro');
        console.log(error);
      }
    );
  }

  getPalestrantes() {
    this.dataImagem = new Date().getMilliseconds().toString();
    this.palestranteService.getPalestrantes().subscribe(
      (palestrantesParam: Palestrante[]) => {
        this.palestrantes = palestrantesParam;
        this.palestrantesFiltrados = this.palestrantes;
      },
      // tslint:disable-next-line: no-shadowed-variable
      (error) => {
        this.toastr.error(
          `Falha ao carregar registros. Mensagem: ${error}`,
          'Erro'
        );
      }
    );
  }

  onFileChange(file: FileList) {
    const reader = new FileReader();
    reader.onload = (event: any) => (this.imagemUrl = event.target.result);
    reader.readAsDataURL(file[0]);
  }

  loadForm(model: Palestrante, template: any) {
    this.openModal(template);
    this.editMode = true;
    this.palestrante = Object.assign({}, model);
    this.palestrante.imagemUrl = '';
    this.fileNameToUpload = model.imagemUrl.toString();
    this.registerForm.patchValue(this.palestrante);
  }

  editarDetalhado(model: Palestrante) {
    this.router.navigate([`/palestrante/${model.id}/edit`]);
  }

  newRegister(template: any) {
    this.openModal(template);
    this.editMode = false;
  }
}
