import {Component, OnInit, inject, signal} from '@angular/core';
import {DatePipe} from '@angular/common';
import { HealthService, HealResponse } from '../../../core/api/health.service';
type Estado = 'carregando' | 'ok' | 'erro';

@Component({
  selector: 'app-status-page',
  imports: [DatePipe],
  templateUrl: './status-page.html',
  styleUrl: './status-page.css',
})
export class StatusPage implements OnInit {
  private readonly health = inject(HealthService);

  protected estado = signal<Estado>('carregando');
  protected readonly resposta = signal<HealResponse | null>(null);

  ngOnInit(): void {
    this.verificar();
}

protected verificar(): void {
  this.estado.set('carregando');
  this.health.check().subscribe({
    next: (r) => {
      this.resposta.set(r);
      this.estado.set('ok');
    },
    error: () => this.estado.set('erro'),
  });
}
}
