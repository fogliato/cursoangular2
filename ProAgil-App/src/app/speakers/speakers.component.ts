import { Component, OnInit } from '@angular/core';
import { Speaker } from '../models/Speaker';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { BsLocaleService } from 'ngx-bootstrap/datepicker';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { SpeakerService } from '../services/speaker.service';
import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';

@Component({
  selector: 'app-speakers',
  templateUrl: './speakers.component.html',
  styleUrls: ['./speakers.component.css'],
})
export class SpeakersComponent implements OnInit {
  title = 'Speakers';
  filteredSpeakers: Speaker[];
  speakers: Speaker[];
  speaker: Speaker;
  imageWidth = 50;
  imageMargin = 2;
  showImage = false;
  editMode = false;
  filterList: string;
  deleteSpeakerBody: string;
  imageUrl = 'assets/img/upload.png';
  modalRef: BsModalRef;
  registerForm: FormGroup;
  file: File;
  fileNameToUpload: string;
  imageTimestamp: string;
  shortBioEditor = ClassicEditor;

  constructor(
    private speakerService: SpeakerService,
    private modalService: BsModalService,
    private fb: FormBuilder,
    private localeService: BsLocaleService,
    private toastr: ToastrService,
    private router: Router
  ) {
    this.localeService.use('en-gb');
  }

  ngOnInit() {
    this.validation();
    this.getSpeakers();
  }

  get filterValue(): string {
    return this.filterList;
  }

  set filterValue(value: string) {
    this.filterList = value;
    this.filteredSpeakers = this.filterValue
      ? this.filterSpeakers(this.filterValue)
      : this.speakers;
  }

  openModal(template: any) {
    this.registerForm.reset();
    template.show();
  }

  filterSpeakers(filterBy: string): Speaker[] {
    filterBy = filterBy.toLocaleLowerCase();
    return this.speakers.filter(
      (speaker) =>
        speaker.name.toLocaleLowerCase().indexOf(filterBy) !== -1 ||
        speaker.email.toLocaleLowerCase().indexOf(filterBy) !== -1
    );
  }

  toggleImage() {
    this.showImage = !this.showImage;
  }

  validation() {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(4)]],
      imageUrl: ['', Validators.required],
      phone: ['', Validators.required],
      shortBio: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
    });
  }

  cancel(template: any) {
    template.hide();
  }

  saveChanges(template: any) {
    if (this.registerForm.valid) {
      if (!this.editMode) {
        this.speaker = Object.assign({}, this.registerForm.value);
        this.uploadAndAdjustFileName();
        this.speakerService.postSpeaker(this.speaker).subscribe(
          (newSpeaker: Speaker) => {
            template.hide();
            this.getSpeakers();
            this.toastr.success(
              'New speaker saved successfully',
              'Success!'
            );
          },
          (error) => {
            this.toastr.error(
              `Failed to save new record to database. Message: ${error}`,
              'Error'
            );
          }
        );
      } else {
        this.speaker = Object.assign(
          { id: this.speaker.id },
          this.registerForm.value
        );
        this.uploadAndAdjustFileName();
        this.speakerService.putSpeaker(this.speaker).subscribe(
          (updatedSpeaker: Speaker) => {
            template.hide();
            this.getSpeakers();
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
    } else {
      this.toastr.error(`Please fill the form correctly.`, 'Error');
    }
  }

  private uploadAndAdjustFileName() {
    if (
      this.registerForm.get('imageUrl').value !== '' &&
      this.registerForm.get('imageUrl').value !== 'assets/img/upload.png'
    ) {
      const fileName = this.speaker.imageUrl.split('\\', 3);
      this.speaker.imageUrl = fileName[2];
      if (!this.editMode) {
        this.fileNameToUpload = this.speaker.imageUrl;
      }
      this.speakerService
        .postUpload(this.file, this.fileNameToUpload)
        .subscribe(() => {
          this.imageTimestamp = new Date().getMilliseconds().toString();
          this.imageUrl = `http://localhost:5000/Resources/Images/${this.speaker.imageUrl}?dtimg=${this.imageTimestamp}`;
        });
    }
  }

  deleteSpeaker(speaker: Speaker, template: any) {
    this.openModal(template);
    this.speaker = speaker;
    this.deleteSpeakerBody = `Are you sure you want to delete the Speaker: ${speaker.name}, Code: ${speaker.id}`;
  }

  confirmDelete(modal: any) {
    this.speakerService.deleteSpeaker(this.speaker.id).subscribe(
      () => {
        modal.hide();
        this.toastr.success(
          'Speaker was deleted successfully.',
          'Success!'
        );
        this.getSpeakers();
      },
      (error) => {
        this.toastr.error('Failed to delete record.', 'Error');
        console.log(error);
      }
    );
  }

  getSpeakers() {
    this.imageTimestamp = new Date().getMilliseconds().toString();
    this.speakerService.getSpeakers().subscribe(
      (speakersParam: Speaker[]) => {
        this.speakers = speakersParam;
        this.filteredSpeakers = this.speakers;
      },
      (error) => {
        this.toastr.error(
          `Failed to load records. Message: ${error}`,
          'Error'
        );
      }
    );
  }

  onFileChange(eventParam: any, file: FileList) {
    const reader = new FileReader();

    reader.onload = (event: any) => (this.imageUrl = event.target.result);

    this.file = eventParam.target.files;
    reader.readAsDataURL(file[0]);
  }

  loadForm(model: Speaker, template: any) {
    this.openModal(template);
    this.editMode = true;
    this.speaker = Object.assign({}, model);
    if (this.speaker.imageUrl !== '') {
      this.imageUrl = `http://localhost:5000/Resources/Images/${this.speaker.imageUrl}?dtimg=${this.imageTimestamp}`;
      this.speaker.imageUrl = '';
    } else {
      this.imageUrl = 'assets/img/upload.png';
    }
    this.fileNameToUpload = model.imageUrl.toString();
    this.registerForm.patchValue(this.speaker);
  }

  editDetailed(model: Speaker) {
    this.router.navigate([`/speaker/${model.id}/edit`]);
  }

  newRegister(template: any) {
    this.openModal(template);
    this.editMode = false;
  }
}

