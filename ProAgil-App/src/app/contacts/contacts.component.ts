import { Component, OnInit } from '@angular/core';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';
import { ChangeEvent } from '@ckeditor/ckeditor5-angular/ckeditor.component';
import { ContactService } from '../services/contact.service';
import { Contact } from '../models/Contact';

@Component({
  selector: 'app-contacts',
  templateUrl: './contacts.component.html',
  styleUrls: ['./contacts.component.css'],
})
export class ContactsComponent implements OnInit {
  registerForm: FormGroup;
  contact: Contact;
  title = 'Contacts';
  public messageEditor = ClassicEditor;
  messageContent = '';

  constructor(
    private contactService: ContactService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.validation();
  }

  executeContact() {
    if (this.registerForm.valid && this.messageContent !== '') {
      this.contact = Object.assign({}, this.registerForm.value);
      this.contact.message = this.messageContent;
      this.contactService.postContact(this.contact).subscribe(
        (newContact: Contact) => {
          this.toastr.success('Contact sent successfully', 'Success');
        },
        (error) => {
          this.toastr.error(`Failed to send contact`, 'Error');
        }
      );
    } else {
      this.toastr.error(`Please fill the form`, 'Error');
    }
  }

  validation() {
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(4)]],
      email: ['', [Validators.required, Validators.email]],
    });
  }

  public onChange({ editor }: ChangeEvent) {
    this.messageContent = editor.getData();
  }
}

