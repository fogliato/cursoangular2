import { Component, OnInit, TemplateRef } from '@angular/core';
import { EventService } from '../services/event.service';
import { Event } from '../models/Event';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { defineLocale } from 'ngx-bootstrap/chronos';
import { enGbLocale } from 'ngx-bootstrap/locale';
import { BsLocaleService } from 'ngx-bootstrap/datepicker';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

defineLocale('en-gb', enGbLocale);
@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  styleUrls: ['./events.component.css'],
})
export class EventsComponent implements OnInit {
  title = 'Events';
  filteredEvents: Event[];
  events: Event[];
  event: Event;
  imageWidth = 50;
  imageMargin = 2;
  showImage = false;
  editMode = false;
  filterList: string;
  deleteEventBody: string;
  eventDate: string;
  modalRef: BsModalRef;
  registerForm: FormGroup;
  file: File;
  fileNameToUpload: string;
  imageTimestamp: string;

  constructor(
    private eventService: EventService,
    private modalService: BsModalService,
    private fb: FormBuilder,
    private localeService: BsLocaleService,
    private toastr: ToastrService,
    private datepipe: DatePipe,
    private router: Router
  ) {
    this.localeService.use('en-gb');
  }

  ngOnInit() {
    this.validation();
    this.getEvents();
  }

  get filterValue(): string {
    return this.filterList;
  }

  set filterValue(value: string) {
    this.filterList = value;
    this.filteredEvents = this.filterValue
      ? this.filterEvents(this.filterValue)
      : this.events;
  }

  openModal(template: any) {
    this.registerForm.reset();
    template.show();
  }

  filterEvents(filterBy: string): Event[] {
    filterBy = filterBy.toLocaleLowerCase();
    return this.events.filter(
      (event) =>
        event.theme.toLocaleLowerCase().indexOf(filterBy) !== -1 ||
        event.location.toLocaleLowerCase().indexOf(filterBy) !== -1
    );
  }

  toggleImage() {
    this.showImage = !this.showImage;
  }

  validation() {
    this.registerForm = this.fb.group({
      theme: ['', [Validators.required, Validators.minLength(4)]],
      location: ['', [Validators.required, Validators.minLength(2)]],
      eventDate: ['', Validators.required],
      peopleCount: ['', Validators.required],
      imageUrl: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
    });
  }

  saveChanges(template: any) {
    if (this.registerForm.valid) {
      if (!this.editMode) {
        this.event = Object.assign({}, this.registerForm.value);
        this.uploadAndAdjustFileName();
        this.eventService.postEvent(this.event).subscribe(
          (newEvent: Event) => {
            template.hide();
            this.getEvents();
            this.toastr.success('New event saved successfully', 'Success!');
          },
          (error) => {
            this.toastr.error(
              `Failed to save new record to database. Message: ${error}`,
              'Error'
            );
          }
        );
      } else {
        this.event = Object.assign(
          { id: this.event.id },
          this.registerForm.value
        );
        this.uploadAndAdjustFileName();
        this.eventService.simpleUpdateEvent(this.event).subscribe(
          (updatedEvent: Event) => {
            template.hide();
            this.getEvents();
            this.toastr.success('Changes saved successfully', 'Success!');
          },
          (error) => {
            this.toastr.error(
              `Failed to update record in database. Message: ${error}`,
              'Error'
            );
          }
        );
      }
    }
  }

  private uploadAndAdjustFileName() {
    const fileName = this.event.imageUrl.split('\\', 3);
    this.event.imageUrl = fileName[2];
    this.imageTimestamp = new Date().getMilliseconds().toString();
    if (this.editMode) {
      this.event.imageUrl = this.fileNameToUpload;
      this.eventService
        .postUpload(this.file, this.fileNameToUpload)
        .subscribe(() => {
          this.getEvents();
        });
    } else {
      this.eventService
        .postUpload(this.file, this.event.imageUrl)
        .subscribe(() => {
          this.getEvents();
        });
    }
  }

  deleteEvent(event: Event, template: any) {
    this.openModal(template);
    this.event = event;
    this.deleteEventBody = `Are you sure you want to delete the Event: ${event.theme}, Code: ${event.id}`;
  }

  confirmDelete(modal: any) {
    this.eventService.deleteEvent(this.event.id).subscribe(
      () => {
        modal.hide();
        this.toastr.success('Event was deleted successfully.', 'Success!');
        this.getEvents();
      },
      (error) => {
        this.toastr.error('Failed to delete record.', 'Error');
        console.log(error);
      }
    );
  }

  getEvents() {
    this.imageTimestamp = new Date().getMilliseconds().toString();
    this.eventService.getEvents().subscribe(
      (eventsParam: Event[]) => {
        this.events = eventsParam;
        this.filteredEvents = this.events;
      },
      (error) => {
        this.toastr.error(
          `Failed to load records. Message: ${error}`,
          'Error'
        );
      }
    );
  }

  onFileChange(event) {
    const reader = new FileReader();
    if (event.target.files && event.target.files.length) {
      this.file = event.target.files;
    }
  }

  loadForm(model: Event, template: any) {
    this.eventDate = this.datepipe.transform(
      model.eventDate,
      'MM/dd/yyyy HH:mm'
    );
    this.openModal(template);
    this.editMode = true;
    this.event = Object.assign({}, model);
    this.event.imageUrl = '';
    this.fileNameToUpload = model.imageUrl.toString();
    this.registerForm.patchValue(this.event);
  }

  editDetailed(model: Event) {
    this.router.navigate([`/event/${model.id}/edit`]);
  }

  newRegister(template: any) {
    this.openModal(template);
    this.editMode = false;
  }
}

