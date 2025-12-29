import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Evento } from '../models/Evento';

@Injectable({
  providedIn: 'root',
})
export class EventoService {
  baseURL = 'http://localhost:5050/api/evento';

  constructor(private http: HttpClient) {}

  getEventos(): Observable<Evento[]> {
    return this.http.get<Evento[]>(this.baseURL);
  }

  getEventoByTema(tema: string): Observable<Evento[]> {
    return this.http.get<Evento[]>(`${this.baseURL}/getbyTema/${tema}`);
  }

  getLatestEventos(): Observable<Evento[]> {
    return this.http.get<Evento[]>(`${this.baseURL}/getLatestEventos`);
  }

  getEventoById(id: number): Observable<Evento> {
    return this.http.get<Evento>(`${this.baseURL}/${id}`);
  }

  postEvento(evento: Evento) {
    return this.http.post(this.baseURL, evento);
  }

  putEvento(evento: Evento) {
    console.log(evento);
    return this.http.put(`${this.baseURL}/${evento.id}`, evento);
  }

  putEventoSimples(evento: Evento) {
    console.log(evento);
    return this.http.put(
      `${this.baseURL}/atualizacaoSimples/${evento.id}`,
      evento
    );
  }

  deleteEvento(id: number) {
    return this.http.delete(`${this.baseURL}/${id}`);
  }

  postUpload(file: File, fileName: string) {
    const fileToUpload = file[0] as File;
    const formData = new FormData();
    formData.append('file', fileToUpload, fileName);

    return this.http.post(`${this.baseURL}/upload`, formData);
  }
}
