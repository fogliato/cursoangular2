// modules
import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgxMaskModule, IConfig } from 'ngx-mask';
import { CommonModule } from '@angular/common';
import { ToastrModule } from 'ngx-toastr';
import { NgxCurrencyModule } from 'ngx-currency';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';

// imports bootstrap angular modules
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { TooltipModule } from 'ngx-bootstrap/tooltip';
import { ModalModule } from 'ngx-bootstrap/modal';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { TabsModule } from 'ngx-bootstrap/tabs';

// components
import { AppComponent } from './app.component';
import { NavComponent } from './nav/nav.component';
import { EventsComponent } from './events/events.component';
import { SpeakersComponent } from './speakers/speakers.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ContactsComponent } from './contacts/contacts.component';
import { TituloComponent } from './shared/titulo/titulo.component';
import { UserComponent } from './user/user.component';
import { LoginComponent } from './user/login/login.component';
import { RegistrationComponent } from './user/registration/registration.component';

// pipes
import { DateTimeFormatPipePipe } from './helpers/DateTimeFormatPipe.pipe';
import { DatePipe } from '@angular/common';

// services
import { EventService } from './services/event.service';
import { AuthInterceptor } from './auth/auth.interceptor';
import { EventEditComponent } from './events/eventEdit/eventEdit.component';

@NgModule({
  declarations: [
    AppComponent,
    TituloComponent,
    EventsComponent,
    NavComponent,
    SpeakersComponent,
    DashboardComponent,
    ContactsComponent,
    DateTimeFormatPipePipe,
    UserComponent,
    LoginComponent,
    RegistrationComponent,
    EventEditComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgxCurrencyModule,
    CKEditorModule,
    FormsModule,
    BsDropdownModule.forRoot(),
    TooltipModule.forRoot(),
    BsDatepickerModule.forRoot(),
    ModalModule.forRoot(),
    BrowserAnimationsModule,
    ReactiveFormsModule,
    NgxMaskModule.forRoot(),
    CommonModule,
    ToastrModule.forRoot({
      timeOut: 5000,
      preventDuplicates: true,
      progressBar: true,
    }),
    TabsModule.forRoot(),
  ],
  providers: [
    EventService,
    DatePipe,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
