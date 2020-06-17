import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from 'src/app/services/auth.service';
import { Login } from 'src/app/models/Login';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  title = 'Login';
  model: any = {};
  loginForm: FormGroup;

  constructor(
    public router: Router,
    private toastr: ToastrService,
    private authService: AuthService,
    public fb: FormBuilder
  ) {}

  ngOnInit() {
    if (this.authService.loggedIn()) {
      this.router.navigate(['/dashboard']);
    } else {
      this.validation();
    }
  }

  validation() {
    this.loginForm = this.fb.group({
      userName: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  login() {
    this.login = Object.assign(this.loginForm.value);
    console.log(this.login);
    this.authService.login(this.model).subscribe(
      (jwt: any) => {
        this.toastr.success('Entrando no sistema', 'Sucesso!');
        this.router.navigate(['/dashboard']);
      },
      (error) => {
        const erro = error.error;
        erro.forEach((element) => {
          switch (element.code) {
            default:
              this.toastr.error(
                `Codigo do erro: ${element.code}`,
                'Erro ao tentar logar no sistema'
              );
              break;
          }
        });
      }
    );
  }
}
