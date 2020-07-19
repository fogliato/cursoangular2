import { Component, OnInit } from '@angular/core';
import { EventoService } from 'src/app/services/evento.service';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { BsLocaleService } from 'ngx-bootstrap/datepicker';
import { ToastrService } from 'ngx-toastr';
import { DatePipe } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { Evento } from 'src/app/models/Evento';

@Component({
  selector: 'app-eventoedit',
  templateUrl: './eventoEdit.component.html',
  styleUrls: ['./eventoEdit.component.css'],
})
export class EventoEditComponent implements OnInit {
  title = 'Eventos - Edição detalhada';
  registerForm: FormGroup;
  evento: Evento = new Evento();
  dataEvento: Date;
  imagemUrl = 'assets/img/upload.png';
  file: File;
  fileNameToUpload: string;
  dataImagem: string;

  get lotes(): FormArray {
    return this.registerForm.get('lotes') as FormArray;
  }

  get redesSociais(): FormArray {
    return this.registerForm.get('redesSociais') as FormArray;
  }

  constructor(
    private eventoService: EventoService,
    private fb: FormBuilder,
    private localeService: BsLocaleService,
    private toastr: ToastrService,
    private datepipe: DatePipe,
    private router: Router,
    private activeRoute: ActivatedRoute
  ) {
    this.localeService.use('pt-br');
  }

  ngOnInit() {
    this.validation();
    this.carregarEvento();
  }

  carregarEvento() {
    const idEvento = +this.activeRoute.snapshot.paramMap.get('id');
    this.eventoService.getEventoById(idEvento).subscribe(
      (eventoResult: Evento) => {
        this.evento = Object.assign({}, eventoResult);
        this.fileNameToUpload = eventoResult.imagemUrl.toString();
        this.dataImagem = new Date().getMilliseconds().toString();
        if (this.evento.imagemUrl !== '') {
          this.imagemUrl = `http://localhost:5000/Resources/Images/${this.evento.imagemUrl}?dtimg=${this.dataImagem}`;
          this.evento.imagemUrl = '';
        } else {
          this.imagemUrl = 'assets/img/upload.png';
        }
        this.registerForm.patchValue(this.evento);
        this.evento.lotes.forEach((lote) => {
          this.lotes.push(this.criaLote(lote));
        });
        this.evento.redesSociais.forEach((redeSocial) => {
          this.redesSociais.push(this.criaRedeSocial(redeSocial));
        });
      },
      // tslint:disable-next-line: no-shadowed-variable
      (error) => {
        this.toastr.error(
          `Falha ao carrega evento. Mensagem: ${error}`,
          'Erro'
        );
      }
    );
  }

  validation() {
    this.registerForm = this.fb.group({
      id: [],
      tema: ['', [Validators.required, Validators.minLength(4)]],
      local: ['', [Validators.required, Validators.minLength(2)]],
      dataEvento: ['', Validators.required],
      qtdPessoas: ['', Validators.required],
      imagemUrl: [''],
      telefone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      lotes: this.fb.array([this.criaLote({ id: 0 })]),
      redesSociais: this.fb.array([this.criaRedeSocial({ id: 0 })]),
    });
  }

  criaLote(lote: any): FormGroup {
    return this.fb.group({
      id: [lote.id],
      nome: [lote.nome, Validators.required],
      quantidade: [lote.quantidade, Validators.required],
      preco: [lote.preco, Validators.required],
      dataInicio: [lote.dataInicio],
      dataFim: [lote.dataFim],
    });
  }

  criaRedeSocial(redeSocial: any): FormGroup {
    return this.fb.group({
      id: [redeSocial.id],
      nome: [redeSocial.nome, Validators.required],
      url: [redeSocial.url],
    });
  }

  adicionarLote() {
    this.lotes.push(this.criaLote({ id: 0 }));
  }

  adicionarRedesSociais() {
    this.redesSociais.push(this.criaRedeSocial({ id: 0 }));
  }

  removerLote(id: number) {
    this.lotes.removeAt(id);
  }

  removerRedeSocial(id: number) {
    this.redesSociais.removeAt(id);
  }

  onFileChange(file: FileList) {
    const reader = new FileReader();
    reader.onload = (event: any) => (this.imagemUrl = event.target.result);
    reader.readAsDataURL(file[0]);
  }

  salvarEvento() {
    this.evento = Object.assign(
      { id: this.evento.id },
      this.registerForm.value
    );
    this.evento.imagemUrl = this.fileNameToUpload;
    this.uploadAndAdjustFileName();

    console.log(this.evento);
    this.eventoService.putEvento(this.evento).subscribe(
      (novo: Evento) => {
        this.toastr.success('Alterações salvas com sucesso', 'Sucesso!');
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

  uploadAndAdjustFileName() {
    if (
      this.registerForm.get('imagemUrl').value !== '' &&
      this.registerForm.get('imagemUrl').value !== 'assets/img/upload.png'
    ) {
      this.eventoService
        .postUpload(this.file, this.fileNameToUpload)
        .subscribe(() => {
          this.dataImagem = new Date().getMilliseconds().toString();
          this.imagemUrl = `http://localhost:5000/Resources/Images/${this.evento.imagemUrl}?dtimg=${this.dataImagem}`;
        });
    }
  }
}
