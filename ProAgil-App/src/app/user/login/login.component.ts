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
    console.log('Login attempt:', this.model);
    this.authService.login(this.model).subscribe(
      (jwt: any) => {
        this.toastr.success('Entrando no sistema', 'Sucesso!');
        this.router.navigate(['/dashboard']);
      },
      (error) => {
        console.error('Login error:', error);
        this.toastr.error('Usuário ou senha inválidos', 'Erro ao logar');
      }
    );
  }
}
