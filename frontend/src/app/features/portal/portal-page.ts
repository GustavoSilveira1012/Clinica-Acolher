import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { AvisoService } from '../../shared/avisos/aviso.service';
import { AppointmentStatus, ROTULO_STATUS } from '../agenda/agenda.models';
import { descreverPolitica } from './politica-texto';
import { PortalApi } from './portal-api';
import { PortalAgenda, PortalPolicy } from './portal.models';

@Component({
  selector: 'app-portal-page',
  imports: [DatePipe],
  templateUrl: './portal-page.html',
  styleUrl: './portal-page.css',
})
export class PortalPage implements OnInit {
  private readonly api = inject(PortalApi);
  private readonly avisos = inject(AvisoService);

  protected readonly agenda = signal<PortalAgenda | null>(null);
  protected readonly politicas = signal<PortalPolicy[]>([]);
  protected readonly concordou = signal<Record<string, boolean>>({});
  protected readonly erro = signal<string | null>(null);

  protected readonly pendentes = computed(() => this.politicas().filter((p) => !p.accepted));
  protected readonly aceitas = computed(() => this.politicas().filter((p) => p.accepted));

  protected readonly descrever = descreverPolitica;

  ngOnInit(): void {
    this.carregar();
  }

  protected rotuloStatus(status: AppointmentStatus): string {
    return ROTULO_STATUS[status];
  }

  protected marcarConcordancia(policyId: string, marcado: boolean): void {
    this.concordou.update((atual) => ({ ...atual, [policyId]: marcado }));
  }

  protected aceitar(politica: PortalPolicy): void {
    this.erro.set(null);
    this.api.acceptPolicy(politica.policyId).subscribe({
      next: (aceita) => {
        this.politicas.update((lista) => lista.map((p) => (p.policyId === aceita.policyId ? aceita : p)));
        this.avisos.mostrar('Política aceita.');
      },
      error: (erro: HttpErrorResponse) => {
        this.erro.set(erro.error?.detail ?? 'Não foi possível registrar o aceite.');
        this.carregar(); // pode existir uma versão mais nova para ler
      },
    });
  }

  private carregar(): void {
    forkJoin({ agenda: this.api.getAgenda(), politicas: this.api.listPolicies() }).subscribe({
      next: ({ agenda, politicas }) => {
        this.agenda.set(agenda);
        this.politicas.set(politicas);
      },
      error: () => this.erro.set('Não foi possível carregar o seu espaço agora. Tente de novo em instantes.'),
    });
  }
}