import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { User } from 'src/app/models/User';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.css'],
})
export class RegistrationComponent implements OnInit {
  registerForm: FormGroup;
  user: User;

  constructor(
    public fb: FormBuilder,
    private toastr: ToastrService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.validation();
  }

  validation() {
    this.registerForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      userName: ['', Validators.required],
      passwords: this.fb.group(
        {
          password: ['', [Validators.required, Validators.minLength(4)]],
          confirmPassword: ['', Validators.required],
        },
        { validator: this.comparePasswords }
      ),
    });
  }

  comparePasswords(fb: FormGroup) {
    const confirmPasswordCtrl = fb.get('confirmPassword');
    if (
      confirmPasswordCtrl.errors == null ||
      'mismatch' in confirmPasswordCtrl.errors
    ) {
      if (fb.get('password').value !== confirmPasswordCtrl.value) {
        confirmPasswordCtrl.setErrors({ mismatch: true });
      } else {
        confirmPasswordCtrl.setErrors(null);
      }
    }
  }

  registerUser() {
    if (this.registerForm.valid) {
      this.user = Object.assign(
        { password: this.registerForm.get('passwords.password').value },
        this.registerForm.value
      );
      this.authService.register(this.user).subscribe(
        (newUser: User) => {
          this.toastr.success(
            'New user registered successfully',
            'Success!'
          );
          this.registerForm.reset();
        },
        (error) => {
          const err = error.error;
          err.forEach((element) => {
            switch (element.code) {
              case 'DuplicateUserName':
                this.toastr.error('Username already exists');
                break;
              default:
                this.toastr.error(
                  `Error code: ${element.code}`,
                  'Registration Error'
                );
                break;
            }
          });
        }
      );
    }
  }
}
