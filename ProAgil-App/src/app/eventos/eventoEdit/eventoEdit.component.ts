import { Component, OnInit } from '@angular/core';
import { EventoService } from 'src/app/services/evento.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BsLocaleService } from 'ngx-bootstrap/datepicker';
import { ToastrService } from 'ngx-toastr';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-eventoedit',
  templateUrl: './eventoEdit.component.html',
  styleUrls: ['./eventoEdit.component.css'],
})
export class EventoEditComponent implements OnInit {
  title = 'Eventos - Edição detalhada';
  registerForm: FormGroup;
  evento: any = {};
  dataEvento: Date;
  imagemURL = 'assets/img/upload.png';
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
      lotes: this.fb.group({
        nome: ['', Validators.required],
        quantidade: ['', Validators.required],
        preco: ['', Validators.required],
        dataInicio: [''],
        dataFim: [''],
      }),
      redesSociais: this.fb.group({
        nome: ['', Validators.required],
        url: [''],
      }),
    });
  }

  onFileChange(file: FileList) {
    const reader = new FileReader();
    reader.onload = (event: any) => this.imagemURL = event.target.result;
    reader.readAsDataURL(File[0]);
  }
}
