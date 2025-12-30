import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Speaker } from '../models/Speaker';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SpeakerService {
  baseURL = 'http://localhost:5050/api/speaker';
  baseUploadURL = 'http://localhost:5050/api/event';

  constructor(private http: HttpClient) {}

  getSpeakers(): Observable<Speaker[]> {
    return this.http.get<Speaker[]>(this.baseURL);
  }

  getSpeakerByName(name: string): Observable<Speaker[]> {
    return this.http.get<Speaker[]>(`${this.baseURL}/getByName/${name}`);
  }

  getSpeakerById(id: number): Observable<Speaker> {
    return this.http.get<Speaker>(`${this.baseURL}/${id}`);
  }

  postSpeaker(speaker: Speaker) {
    return this.http.post(this.baseURL, speaker);
  }

  putSpeaker(speaker: Speaker) {
    console.log(speaker);
    return this.http.put(`${this.baseURL}/${speaker.id}`, speaker);
  }

  deleteSpeaker(id: number) {
    return this.http.delete(`${this.baseURL}/${id}`);
  }

  postUpload(file: File, fileName: string) {
    const fileToUpload = file[0] as File;
    const formData = new FormData();
    formData.append('file', fileToUpload, fileName);

    return this.http.post(`${this.baseUploadURL}/upload`, formData);
  }
}

