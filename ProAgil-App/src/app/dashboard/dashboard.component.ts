import { Component, OnInit } from '@angular/core';
import { EventService } from '../services/event.service';
import { Event } from '../models/Event';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  title = 'Home';
  events: Event[];
  constructor(
    private eventService: EventService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.getEvents();
  }

  getEvents() {
    this.eventService.getLatestEvents().subscribe(
      (eventsParam: Event[]) => {
        this.events = eventsParam;
      },
      (error) => {
        this.toastr.error(
          `Failed to load records. Message: ${error}`,
          'Error'
        );
      }
    );
  }

}
