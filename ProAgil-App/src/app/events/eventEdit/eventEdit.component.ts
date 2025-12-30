import { Component, OnInit } from '@angular/core';
import { EventService } from 'src/app/services/event.service';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { BsLocaleService } from 'ngx-bootstrap/datepicker';
import { ToastrService } from 'ngx-toastr';
import { DatePipe } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { Event } from 'src/app/models/Event';
import { Batch } from 'src/app/models/Batch';

@Component({
  selector: 'app-eventedit',
  templateUrl: './eventEdit.component.html',
  styleUrls: ['./eventEdit.component.css'],
})
export class EventEditComponent implements OnInit {
  title = 'Events - Detailed Edit';
  registerForm: FormGroup;
  event: Event = new Event();
  eventDate: Date;
  imageUrl = 'assets/img/upload.png';
  file: File;
  fileNameToUpload: string;
  imageTimestamp: string;

  get batches(): FormArray {
    return this.registerForm.get('batches') as FormArray;
  }

  get socialNetworks(): FormArray {
    return this.registerForm.get('socialNetworks') as FormArray;
  }

  constructor(
    private eventService: EventService,
    private fb: FormBuilder,
    private localeService: BsLocaleService,
    private toastr: ToastrService,
    private datepipe: DatePipe,
    private router: Router,
    private activeRoute: ActivatedRoute
  ) {
    this.localeService.use('en-gb');
  }

  ngOnInit() {
    this.validation();
    this.loadEvent();
  }

  loadEvent() {
    this.removeBatch(0);
    this.removeSocialNetwork(0);
    const eventId = +this.activeRoute.snapshot.paramMap.get('id');
    this.eventService.getEventById(eventId).subscribe(
      (eventResult: Event) => {
        this.event = Object.assign({}, eventResult);
        this.fileNameToUpload = eventResult.imageUrl.toString();
        this.imageTimestamp = new Date().getMilliseconds().toString();
        if (this.event.imageUrl !== '') {
          this.imageUrl = `http://localhost:5000/Resources/Images/${this.event.imageUrl}?dtimg=${this.imageTimestamp}`;
          this.event.imageUrl = '';
        } else {
          this.imageUrl = 'assets/img/upload.png';
        }
        this.registerForm.patchValue(this.event);
        this.event.batches.forEach((batch) => {
          this.batches.push(this.createBatch(batch));
        });
        this.event.socialNetworks.forEach((socialNetwork) => {
          this.socialNetworks.push(this.createSocialNetwork(socialNetwork));
        });
      },
      (error) => {
        this.toastr.error(
          `Failed to load event. Message: ${error}`,
          'Error'
        );
      }
    );
  }

  validation() {
    this.registerForm = this.fb.group({
      id: [],
      theme: ['', [Validators.required, Validators.minLength(4)]],
      location: ['', [Validators.required, Validators.minLength(2)]],
      eventDate: ['', Validators.required],
      peopleCount: ['', Validators.required],
      imageUrl: [''],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      batches: this.fb.array([this.createBatch({ id: 0 })]),
      socialNetworks: this.fb.array([this.createSocialNetwork({ id: 0 })]),
    });
  }

  createBatch(batch: any): FormGroup {
    return this.fb.group({
      id: [batch.id],
      name: [batch.name, Validators.required],
      quantity: [batch.quantity, Validators.required],
      price: [batch.price, Validators.required],
      startDate: [this.datepipe.transform(batch.startDate, 'yyyy-MM-dd')],
      endDate: [this.datepipe.transform(batch.endDate, 'yyyy-MM-dd')],
    });
  }

  createSocialNetwork(socialNetwork: any): FormGroup {
    return this.fb.group({
      id: [socialNetwork.id],
      name: [socialNetwork.name, Validators.required],
      url: [socialNetwork.url],
    });
  }

  addBatch() {
    this.batches.push(this.createBatch({ id: 0 }));
  }

  addSocialNetwork() {
    this.socialNetworks.push(this.createSocialNetwork({ id: 0 }));
  }

  removeBatch(id: number) {
    this.batches.removeAt(id);
  }

  removeSocialNetwork(id: number) {
    this.socialNetworks.removeAt(id);
  }

  onFileChange(eventParam: any, file: FileList) {
    const reader = new FileReader();

    reader.onload = (readerEvent: any) => this.imageUrl = readerEvent.target.result;

    this.file = eventParam.target.files;
    reader.readAsDataURL(file[0]);
  }

  saveEvent() {
    this.event = Object.assign(
      { id: this.event.id },
      this.registerForm.value
    );
    this.event.imageUrl = this.fileNameToUpload;
    this.uploadAndAdjustFileName();

    this.eventService.putEvent(this.event).subscribe(
      (updatedEvent: Event) => {
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

  uploadAndAdjustFileName() {
    if (
      this.registerForm.get('imageUrl').value !== '' &&
      this.registerForm.get('imageUrl').value !== 'assets/img/upload.png'
    ) {
      this.eventService
        .postUpload(this.file, this.fileNameToUpload)
        .subscribe(() => {
          this.imageTimestamp = new Date().getMilliseconds().toString();
          this.imageUrl = `http://localhost:5000/Resources/Images/${this.event.imageUrl}?dtimg=${this.imageTimestamp}`;
        });
    }
  }
}

