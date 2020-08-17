import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Palestrante } from '../models/Palestrante';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PalestranteService {
  baseURL = 'http://localhost:5000/api/palestrante';
  baseUploadURL = 'http://localhost:5000/api/evento';

  constructor(private http: HttpClient) {}

  getPalestrantes(): Observable<Palestrante[]> {
    return this.http.get<Palestrante[]>(this.baseURL);
  }

  getPalestranteByNome(nome: string): Observable<Palestrante[]> {
    return this.http.get<Palestrante[]>(`${this.baseURL}/getByName/${nome}`);
  }

  getPalestranteById(id: number): Observable<Palestrante> {
    return this.http.get<Palestrante>(`${this.baseURL}/${id}`);
  }

  postPalestrante(palestrante: Palestrante) {
    return this.http.post(this.baseURL, palestrante);
  }

  putPalestrante(palestrante: Palestrante) {
    console.log(palestrante);
    return this.http.put(`${this.baseURL}/${palestrante.id}`, palestrante);
  }

  deletePalestrante(id: number) {
    return this.http.delete(`${this.baseURL}/${id}`);
  }

  postUpload(file: File, fileName: string) {
    const fileToUpload = file[0] as File;
    const formData = new FormData();
    formData.append('file', fileToUpload, fileName);

    return this.http.post(`${this.baseUploadURL}/upload`, formData);
  }
}
