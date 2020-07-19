import { Component, OnInit, TemplateRef } from '@angular/core';
import { EventoService } from '../services/evento.service';
import { Evento } from '../models/Evento';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { defineLocale } from 'ngx-bootstrap/chronos';
import { ptBrLocale } from 'ngx-bootstrap/locale';
import { BsLocaleService } from 'ngx-bootstrap/datepicker';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { error } from 'protractor';
import { ToastrService } from 'ngx-toastr';
import { timingSafeEqual } from 'crypto';
import { Router } from '@angular/router';

defineLocale('pt-br', ptBrLocale);
@Component({
  selector: 'app-eventos',
  templateUrl: './eventos.component.html',
  styleUrls: ['./eventos.component.css'],
})
export class EventosComponent implements OnInit {
  title = 'Eventos';
  eventosFiltrados: Evento[];
  eventos: Evento[];
  evento: Evento;
  imagemLargura = 50;
  imagemMargem = 2;
  mostrarImagem = false;
  editMode = false;
  filtroList: string;
  bodyDeletarEvento: string;
  dataEvento: string;
  modalRef: BsModalRef;
  registerForm: FormGroup;
  file: File;
  fileNameToUpload: string;
  dataImagem: string;

  constructor(
    private eventoService: EventoService,
    private modalService: BsModalService,
    private fb: FormBuilder,
    private localeService: BsLocaleService,
    private toastr: ToastrService,
    private datepipe: DatePipe,
    private router: Router
  ) {
    this.localeService.use('pt-br');
  }

  ngOnInit() {
    this.validation();
    this.getEventos();
  }

  get filtroLista(): string {
    return this.filtroList;
  }

  set filtroLista(value: string) {
    this.filtroList = value;
    this.eventosFiltrados = this.filtroLista
      ? this.filtrarEventos(this.filtroLista)
      : this.eventos;
  }

  openModal(template: any) {
    this.registerForm.reset();
    template.show();
  }

  filtrarEventos(filtrarPor: string): Evento[] {
    filtrarPor = filtrarPor.toLocaleLowerCase();
    return this.eventos.filter(
      (evento) =>
        evento.tema.toLocaleLowerCase().indexOf(filtrarPor) !== -1 ||
        evento.local.toLocaleLowerCase().indexOf(filtrarPor) !== -1
    );
  }

  alternarImagem() {
    this.mostrarImagem = !this.mostrarImagem;
  }

  validation() {
    this.registerForm = this.fb.group({
      tema: ['', [Validators.required, Validators.minLength(4)]],
      local: ['', [Validators.required, Validators.minLength(2)]],
      dataEvento: ['', Validators.required],
      qtdPessoas: ['', Validators.required],
      imagemUrl: ['', Validators.required],
      telefone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
    });
  }

  salvarAlteracao(template: any) {
    if (this.registerForm.valid) {
      if (!this.editMode) {
        this.evento = Object.assign({}, this.registerForm.value);
        this.uploadAndAdjustFileName();
        this.eventoService.postEvento(this.evento).subscribe(
          (novo: Evento) => {
            template.hide();
            this.getEventos();
            this.toastr.success('Novo evento salvo com sucesso', 'Sucesso!');
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
        this.evento = Object.assign(
          { id: this.evento.id },
          this.registerForm.value
        );
        this.uploadAndAdjustFileName();
        this.eventoService.putEvento(this.evento).subscribe(
          (novo: Evento) => {
            template.hide();
            this.getEventos();
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
    }
  }

  private uploadAndAdjustFileName() {
    const nomeArquivo = this.evento.imagemUrl.split('\\', 3);
    this.evento.imagemUrl = nomeArquivo[2];
    this.dataImagem = new Date().getMilliseconds().toString();
    if (this.editMode) {
      this.evento.imagemUrl = this.fileNameToUpload;
      this.eventoService
        .postUpload(this.file, this.fileNameToUpload)
        .subscribe(() => {
          this.getEventos();
        });
    } else {
      this.eventoService
        .postUpload(this.file, this.evento.imagemUrl)
        .subscribe(() => {
          this.getEventos();
        });
    }
  }

  excluirEvento(evento: Evento, template: any) {
    this.openModal(template);
    this.evento = evento;
    this.bodyDeletarEvento = `Tem certeza que deseja excluir o Evento: ${evento.tema}, Código: ${evento.id}`;
  }

  confirmeDelete(modal: any) {
    this.eventoService.deleteEvento(this.evento.id).subscribe(
      () => {
        modal.hide();
        this.toastr.success('O Evento foi excluído com sucesso.', 'Sucesso!');
        this.getEventos();
      },
      // tslint:disable-next-line: no-shadowed-variable
      (error) => {
        this.toastr.error('Falha ao excluir registro.', 'Erro');
        console.log(error);
      }
    );
  }

  getEventos() {
    this.dataImagem = new Date().getMilliseconds().toString();
    this.eventoService.getEventos().subscribe(
      (eventosParam: Evento[]) => {
        this.eventos = eventosParam;
        this.eventosFiltrados = this.eventos;
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

  onFileChange(event) {
    const reader = new FileReader();
    if (event.target.files && event.target.files.length) {
      this.file = event.target.files;
    }
  }

  loadForm(model: Evento, template: any) {
    this.dataEvento = this.datepipe.transform(
      model.dataEvento,
      'dd/MM/yyyy HH:mm'
    );
    this.openModal(template);
    this.editMode = true;
    this.evento = Object.assign({}, model);
    this.evento.imagemUrl = '';
    this.fileNameToUpload = model.imagemUrl.toString();
    this.registerForm.patchValue(this.evento);
  }

  editarDetalhado(model: Evento) {
    this.router.navigate([`/evento/${model.id}/edit`]);
  }

  newRegister(template: any) {
    this.openModal(template);
    this.editMode = false;
  }
}
