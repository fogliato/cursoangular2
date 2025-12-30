import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Contact } from '../models/Contact';

@Injectable({
  providedIn: 'root',
})
export class ContactService {
  baseURL = 'http://localhost:5050/api/contact';

  constructor(private http: HttpClient) {}

  postContact(contact: Contact) {
    return this.http.post(this.baseURL, contact);
  }
}

