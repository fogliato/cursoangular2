import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { JwtHelperService } from '@auth0/angular-jwt';
import { map } from 'rxjs/operators';
import { User } from '../models/User';
import { Login } from '../models/Login';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  baseURL = 'http://localhost:5000/api/user/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;

  constructor(private http: HttpClient) {}

  login(model: Login) {
    return this.http.post(`${this.baseURL}login`, model).pipe(
      map((response: any) => {
        const user = response;
        if (user) {
          localStorage.setItem('token', user.token);
          this.decodedToken = this.jwtHelper.decodeToken(user.token);
        }
      })
    );
  }

  register(model: User) {
    return this.http.post(`${this.baseURL}register`, model);
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    if (token == null) {
      return false;
    }
    return !this.jwtHelper.isTokenExpired(token);
  }

  logOut() {
    const token = localStorage.clear();
  }
}
