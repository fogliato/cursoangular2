import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';
import { ChangeEvent } from '@ckeditor/ckeditor5-angular/ckeditor.component';
import { ContatoService } from '../services/contato.service';
import { Contato } from '../models/Contato';

@Component({
  selector: 'app-contatos',
  templateUrl: './contatos.component.html',
  styleUrls: ['./contatos.component.css'],
})
export class ContatosComponent implements OnInit {
  registerForm: FormGroup;
  contato: Contato;
  title = 'Contatos';
  public mensagem = ClassicEditor;
  conteudoMensagem = '';

  constructor(
    private contatoService: ContatoService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.validation();
  }

  executarContato() {
    if (this.registerForm.valid && this.conteudoMensagem !== '') {
      this.contato = Object.assign({}, this.registerForm.value);
      this.contato.mensagem = this.conteudoMensagem;
      console.log(this.contato);
      this.contatoService.postContato(this.contato).subscribe(
        (novo: Contato) => {
          this.toastr.success('Contato realizado com sucesso', 'Sucesso');
        },
        (error) => {
          this.toastr.error(`Falha ao realizar o contato`, 'Erro');
        }
      );
    } else {
      this.toastr.error(`Preencha o formulário`, 'Erro');
    }
  }

  validation() {
    this.registerForm = this.fb.group({
      nomeCompleto: ['', [Validators.required, Validators.minLength(4)]],
      email: ['', [Validators.required, Validators.email]],
    });
  }

  public onChange({ editor }: ChangeEvent) {
    this.conteudoMensagem = editor.getData();
  }
}
