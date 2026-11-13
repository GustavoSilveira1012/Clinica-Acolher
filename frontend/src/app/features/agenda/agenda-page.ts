import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { AgendaApi } from './agenda-api';
import { AgendaItem, AppointmentStatus, CancellationSource, ChargeReason, ProfessionalSummary } from './agenda.models';
import { addDays, dateKey, startOfWeek } from './week';

const ROTULO_STATUS: Record<AppointmentStatus, string> = {
  Scheduled: 'Agendada',
  Confirmed: 'Confirmada',
  Completed: 'Realizada',
  NoShow: 'Falta',
  Cancelled: 'Cancelada',
  Rescheduled: 'Remarcada',
};

const ROTULO_COBRANCA: Record<ChargeReason, string> = {
  None: '',
  LateCancellation: 'cancelamento tardio',
  NoShow: 'falta',
};

@Component({
  selector: 'app-agenda-page',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './agenda-page.html',
  styleUrl: './agenda-page.css',
})
export class AgendaPage implements OnInit {
  private readonly api = inject(AgendaApi);

  protected readonly profissionais = signal<ProfessionalSummary[]>([]);
  protected readonly profissionalId = signal<string | null>(null);
  protected readonly inicioSemana = signal(startOfWeek(new Date()));
  protected readonly itens = signal<AgendaItem[]>([]);
  protected readonly carregando = signal(false);
  protected readonly mensagemErro = signal<string | null>(null);
  protected readonly cancelando = signal<AgendaItem | null>(null);

  protected readonly formCancelamento = new FormGroup({
    quem: new FormControl<CancellationSource>('Patient', { nonNullable: true }),
    motivo: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(500)] }),
  });

  protected readonly fimSemana = computed(() => addDays(this.inicioSemana(), 6));

  protected readonly dias = computed(() => {
    const inicio = this.inicioSemana();
    const itens = this.itens();
    return Array.from({ length: 7 }, (_, i) => {
      const data = addDays(inicio, i);
      const chave = dateKey(data);
      return { data, chave, itens: itens.filter((item) => dateKey(new Date(item.startsAt)) === chave) };
    });
  });

  ngOnInit(): void {
    this.api.listProfessionals().subscribe({
      next: (lista) => {
        const aptos = lista.filter((p) => p.canAttend);
        this.profissionais.set(aptos);
        if (aptos.length > 0) {
          this.profissionalId.set(aptos[0].id);
          this.carregar();
        }
      },
      error: () => this.mensagemErro.set('Não foi possível carregar os profissionais.'),
    });
  }

  protected escolherProfissional(id: string): void {
    this.profissionalId.set(id);
    this.carregar();
  }

  protected mudarSemana(semanas: number): void {
    this.inicioSemana.update((inicio) => addDays(inicio, semanas * 7));
    this.carregar();
  }

  protected irParaHoje(): void {
    this.inicioSemana.set(startOfWeek(new Date()));
    this.carregar();
  }

  protected rotuloStatus(item: AgendaItem): string {
    return ROTULO_STATUS[item.status];
  }

  protected rotuloCobranca(item: AgendaItem): string {
    return ROTULO_COBRANCA[item.chargeReason];
  }

  protected ativa(item: AgendaItem): boolean {
    return item.status === 'Scheduled' || item.status === 'Confirmed';
  }

  protected jaComecou(item: AgendaItem): boolean {
    return new Date(item.startsAt).getTime() <= Date.now();
  }

  protected confirmar(item: AgendaItem): void {
    this.executar(this.api.confirm(item.id));
  }

  protected registrarRealizada(item: AgendaItem): void {
    this.executar(this.api.complete(item.id));
  }

  protected registrarFalta(item: AgendaItem): void {
    this.executar(this.api.noShow(item.id));
  }

  protected iniciarCancelamento(item: AgendaItem): void {
    this.formCancelamento.reset();
    this.cancelando.set(item);
  }

  protected desistirCancelamento(): void {
    this.cancelando.set(null);
  }

  protected confirmarCancelamento(): void {
    const item = this.cancelando();
    const { quem, motivo } = this.formCancelamento.getRawValue();
    if (!item) return;
    if (!motivo.trim()) {
      this.formCancelamento.markAllAsTouched();
      return;
    }
    this.executar(this.api.cancel(item.id, quem, motivo.trim()), () => this.cancelando.set(null));
  }

  private carregar(): void {
    const id = this.profissionalId();
    const inicio = this.inicioSemana();
    if (!id) return;

    this.carregando.set(true);
    this.api.getAgenda(id, inicio, addDays(inicio, 7)).subscribe({
      next: (itens) => {
        // Uma resposta atrasada de outra semana não pode sobrescrever a semana atual
        if (this.inicioSemana() !== inicio || this.profissionalId() !== id) return;
        this.itens.set(itens);
        this.carregando.set(false);
      },
      error: () => {
        this.mensagemErro.set('Não foi possível carregar a agenda.');
        this.carregando.set(false);
      },
    });
  }

  private executar(chamada: Observable<unknown>, depois?: () => void): void {
    this.mensagemErro.set(null);
    chamada.subscribe({
      next: () => {
        depois?.();
        this.carregar();
      },
      error: (erro: HttpErrorResponse) =>
        this.mensagemErro.set(erro.error?.detail ?? 'Não foi possível concluir a ação.'),
    });
  }
}