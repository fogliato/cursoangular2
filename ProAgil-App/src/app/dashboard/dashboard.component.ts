import { Component, OnInit } from '@angular/core';
import { EventoService } from '../services/evento.service';
import { Evento } from '../models/Evento';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  title = 'Início';
  eventos: Evento[];
  constructor(
    private eventoService: EventoService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.getEventos();
  }

  getEventos() {
    this.eventoService.getLatestEventos().subscribe(
      (eventosParam: Evento[]) => {
        this.eventos = eventosParam;
      },
      // tslint:disable-next-line: no-shadowed-variable
      (error) => {
        this.toastr.error(
          `Falha ao carregar registros. Mensagem: ${error}`,
          'Erro'
        );
      }
    );
  }

}
