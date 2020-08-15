import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Contato } from '../models/Contato';

@Injectable({
  providedIn: 'root',
})
export class ContatoService {
  baseURL = 'http://localhost:5000/api/contato';

  constructor(private http: HttpClient) {}

  postContato(contato: Contato) {
    return this.http.post(this.baseURL, contato);
  }
}
