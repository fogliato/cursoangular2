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
        this.evento.imagemUrl = '';
        this.fileNameToUpload = eventoResult.imagemUrl.toString();
        this.dataImagem = new Date().getMilliseconds().toString();
        this.imagemUrl = `http://localhost:5000/Resources/Images/${this.evento.imagemUrl}?dtimg=${this.dataImagem}`;
        this.registerForm.patchValue(this.evento);
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
      tema: ['', [Validators.required, Validators.minLength(4)]],
      local: ['', [Validators.required, Validators.minLength(2)]],
      dataEvento: ['', Validators.required],
      qtdPessoas: ['', Validators.required],
      imagemUrl: [''],
      telefone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      lotes: this.fb.array([this.criaLote()]),
      redesSociais: this.fb.array([this.criaRedeSocial()]),
    });
  }

  criaLote(): FormGroup {
    return this.fb.group({
      nome: ['', Validators.required],
      quantidade: ['', Validators.required],
      preco: ['', Validators.required],
      dataInicio: [''],
      dataFim: [''],
    });
  }

  criaRedeSocial(): FormGroup {
    return this.fb.group({
      nome: ['', Validators.required],
      url: [''],
    });
  }

  adicionarLote() {
    this.lotes.push(this.criaLote());
  }

  adicionarRedesSociais() {
    this.redesSociais.push(this.criaRedeSocial());
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
}
