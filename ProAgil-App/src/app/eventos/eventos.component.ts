import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-eventos',
  templateUrl: './eventos.component.html',
  styleUrls: ['./eventos.component.css'],
})
export class EventosComponent implements OnInit {
  filtroList: string;
  get filtroLista(): string {
    return this.filtroList;
  }
  set filtroLista(value: string) {
    this.filtroList = value;
    this.eventosFiltrados = this.filtroLista
      ? this.filtrarEventos(this.filtroLista)
      : this.eventos;
  }
  eventosFiltrados: any = [];
  eventos: any = [];
  imagemLargura = 50;
  imagemMargem = 2;
  mostrarImagem = false;

  constructor(private http: HttpClient) {}

  filtrarEventos(filtrarPor: string): any {
    filtrarPor = filtrarPor.toLocaleLowerCase();
    return this.eventos.filter(
      (evento) => evento.tema.toLocaleLowerCase().indexOf(filtrarPor) !== -1 ||
                  evento.local.toLocaleLowerCase().indexOf(filtrarPor) !== -1
    );
  }

  ngOnInit() {
    this.getEventos();
  }

  alternarImagem() {
    this.mostrarImagem = !this.mostrarImagem;
  }

  getEventos() {
    this.http.get('http://localhost:5000/api/values').subscribe(
      (response) => {
        this.eventos = response;
      },
      (error) => {
        console.log(error);
      }
    );
  }
}
