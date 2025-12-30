import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Event } from '../models/Event';

@Injectable({
  providedIn: 'root',
})
export class EventService {
  baseURL = 'http://localhost:5050/api/event';

  constructor(private http: HttpClient) {}

  getEvents(): Observable<Event[]> {
    return this.http.get<Event[]>(this.baseURL);
  }

  getEventByTheme(theme: string): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.baseURL}/getByTheme/${theme}`);
  }

  getLatestEvents(): Observable<Event[]> {
    return this.http.get<Event[]>(`${this.baseURL}/getLatestEvents`);
  }

  getEventById(id: number): Observable<Event> {
    return this.http.get<Event>(`${this.baseURL}/${id}`);
  }

  postEvent(event: Event) {
    return this.http.post(this.baseURL, event);
  }

  putEvent(event: Event) {
    console.log(event);
    return this.http.put(`${this.baseURL}/${event.id}`, event);
  }

  simpleUpdateEvent(event: Event) {
    console.log(event);
    return this.http.put(
      `${this.baseURL}/simpleUpdate/${event.id}`,
      event
    );
  }

  deleteEvent(id: number) {
    return this.http.delete(`${this.baseURL}/${id}`);
  }

  postUpload(file: File, fileName: string) {
    const fileToUpload = file[0] as File;
    const formData = new FormData();
    formData.append('file', fileToUpload, fileName);

    return this.http.post(`${this.baseURL}/upload`, formData);
  }
}

