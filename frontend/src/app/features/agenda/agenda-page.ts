import { Component, OnInit, Signal, computed, inject, signal } from '@angular/core';
import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { Observable, catchError, debounceTime, forkJoin, of, switchMap } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { AvisoService } from '../../shared/avisos/aviso.service';
import { Dialogo } from '../../shared/dialogo/dialogo';
import { AvailabilityApi } from '../availability/availability-api';
import { AvailabilityBlock, AvailabilityWindow } from '../availability/availability.models';
import { AgendaApi } from './agenda-api';
import { AgendaGrade } from './agenda-grade';
import {
  ActiveRelationship, AgendaItem, CancellationSource, ProfessionalSummary, ROTULO_COBRANCA, ROTULO_STATUS,
} from './agenda.models';
import { periodosDoDia } from './grade';
import { addDays, dateKey, nextFullHour, startOfWeek, toLocalInputValue } from './week';

type Modo = 'semana' | 'dia';

// O navegador simulado dos testes não tem matchMedia; sem ele, a agenda abre na semana
function telaEstreita(): boolean {
  return typeof window.matchMedia === 'function' && window.matchMedia('(max-width: 760px)').matches;
}

@Component({
  selector: 'app-agenda-page',
  imports: [DatePipe, NgTemplateOutlet, ReactiveFormsModule, Dialogo, AgendaGrade],
  templateUrl: './agenda-page.html',
  styleUrl: './agenda-page.css',
})
export class AgendaPage implements OnInit {
  private readonly api = inject(AgendaApi);
  private readonly disponibilidadeApi = inject(AvailabilityApi);
  private readonly auth = inject(AuthService);
  private readonly avisos = inject(AvisoService);
  private readonly fusoHorario = Intl.DateTimeFormat().resolvedOptions().timeZone;

  protected readonly profissionais = signal<ProfessionalSummary[]>([]);
  protected readonly profissionalId = signal<string | null>(null);
  protected readonly modo = signal<Modo>(telaEstreita() ? 'dia' : 'semana');
  protected readonly diaSelecionado = signal(addDays(new Date(), 0));
  protected readonly inicioSemana = computed(() => startOfWeek(this.diaSelecionado()));
  protected readonly fimSemana = computed(() => addDays(this.inicioSemana(), 6));

  protected readonly itens = signal<AgendaItem[]>([]);
  protected readonly periodos = signal<AvailabilityWindow[]>([]);
  protected readonly bloqueios = signal<AvailabilityBlock[]>([]);
  private readonly duracaoPadrao = signal(50);
  protected readonly carregando = signal(false);
  protected readonly mensagemErro = signal<string | null>(null);

  protected readonly selecionada = signal<AgendaItem | null>(null);
  protected readonly agendando = signal(false);
  protected readonly remarcando = signal<AgendaItem | null>(null);
  protected readonly cancelando = signal<AgendaItem | null>(null);
  protected readonly encerrandoSerie = signal<AgendaItem | null>(null);
  protected readonly vinculos = signal<ActiveRelationship[]>([]);

  protected readonly algumDialogoAberto = computed(
    () => this.agendando() || !!this.selecionada() || !!this.remarcando() || !!this.cancelando() || !!this.encerrandoSerie(),
  );

  protected readonly formAgendamento = new FormGroup({
    vinculoId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    inicio: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    duracao: new FormControl(50, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(15), Validators.max(180)],
    }),
    repetir: new FormControl(false, { nonNullable: true }),
  });

  protected readonly formRemarcacao = new FormGroup({
    inicio: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    duracao: new FormControl(50, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(15), Validators.max(180)],
    }),
  });

  protected readonly formCancelamento = new FormGroup({
    quem: new FormControl<CancellationSource>('Patient', { nonNullable: true }),
    motivo: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(500)] }),
  });

  // RN-AGD-13: o aviso aparece enquanto a profissional escolhe o horário
  protected readonly alertaAgendamento = this.alertaDeHorario(this.formAgendamento);
  protected readonly alertaRemarcacao = this.alertaDeHorario(this.formRemarcacao);

  protected readonly diasDaSemana = computed(() => {
    const inicio = this.inicioSemana();
    // Sábado e domingo só aparecem quando têm atendimento ou sessões
    return Array.from({ length: 7 }, (_, i) => addDays(inicio, i)).filter((dia, i) => i < 5 || this.temAlgoNoDia(dia));
  });

  protected readonly itensDaGrade = computed(() =>
    this.itens().filter((i) => i.status !== 'Cancelled' && i.status !== 'Rescheduled'),
  );

  protected readonly itensDoDia = computed(() => {
    const chave = dateKey(this.diaSelecionado());
    return this.itens().filter((i) => dateKey(new Date(i.startsAt)) === chave);
  });

  ngOnInit(): void {
    this.api.listProfessionals().subscribe({
      next: (lista) => {
        const usuario = this.auth.user();
        const ehAdministracao = usuario?.roles.includes('Admin') ?? false;
        const aptos = lista.filter(
          (p) => p.canAttend && (!usuario || ehAdministracao || p.id === usuario.professionalId),
        );
        this.profissionais.set(aptos);
        if (aptos.length > 0) this.escolherProfissional(aptos[0].id);
      },
      error: () => this.mensagemErro.set('Não foi possível carregar os profissionais.'),
    });
  }

  protected escolherProfissional(id: string): void {
    this.profissionalId.set(id);
    this.carregarDisponibilidade();
    this.carregar();
  }

  protected navegar(passo: number): void {
    const semanaAntes = dateKey(this.inicioSemana());
    this.diaSelecionado.update((dia) => addDays(dia, this.modo() === 'semana' ? passo * 7 : passo));
    if (dateKey(this.inicioSemana()) !== semanaAntes) this.carregar();
  }

  protected irParaHoje(): void {
    const semanaAntes = dateKey(this.inicioSemana());
    this.diaSelecionado.set(addDays(new Date(), 0));
    if (dateKey(this.inicioSemana()) !== semanaAntes) this.carregar();
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

  // Janelas

  protected selecionar(item: AgendaItem): void {
    this.fecharPaineis();
    this.selecionada.set(item);
  }

  protected abrirAgendamento(): void {
    const id = this.profissionalId();
    if (!id) return;
    this.fecharPaineis();
    this.formAgendamento.reset({
      vinculoId: '',
      inicio: toLocalInputValue(nextFullHour(new Date())),
      duracao: this.duracaoPadrao(),
      repetir: false,
    });
    this.agendando.set(true);
    this.api.listActiveRelationships(id).subscribe({
      next: (lista) => this.vinculos.set(lista),
      error: () => this.mensagemErro.set('Não foi possível carregar os pacientes do profissional.'),
    });
  }

  protected abrirRemarcacao(item: AgendaItem): void {
    this.fecharPaineis();
    const inicio = new Date(item.startsAt);
    const duracao = Math.round((new Date(item.endsAt).getTime() - inicio.getTime()) / 60_000);
    this.formRemarcacao.reset({ inicio: toLocalInputValue(inicio), duracao });
    this.remarcando.set(item);
  }

  protected iniciarCancelamento(item: AgendaItem): void {
    this.fecharPaineis();
    this.formCancelamento.reset();
    this.cancelando.set(item);
  }

  protected abrirEncerramentoSerie(item: AgendaItem): void {
    this.fecharPaineis();
    this.encerrandoSerie.set(item);
  }

  protected fecharPaineis(): void {
    this.selecionada.set(null);
    this.agendando.set(false);
    this.remarcando.set(null);
    this.cancelando.set(null);
    this.encerrandoSerie.set(null);
    this.mensagemErro.set(null);
  }

  // Ações

  protected confirmarAgendamento(): void {
    if (this.formAgendamento.invalid) {
      this.formAgendamento.markAllAsTouched();
      return;
    }
    const { vinculoId, inicio, duracao, repetir } = this.formAgendamento.getRawValue();
    const data = new Date(inicio);
    const chamada = repetir
      ? this.api.createSeries(vinculoId, data, duracao, this.fusoHorario)
      : this.api.schedule(vinculoId, data, duracao);
    this.executar(chamada, repetir ? 'Série criada: as próximas 12 semanas foram agendadas.' : 'Sessão agendada.');
  }

  protected confirmarRemarcacao(): void {
    const item = this.remarcando();
    if (!item) return;
    if (this.formRemarcacao.invalid) {
      this.formRemarcacao.markAllAsTouched();
      return;
    }
    const { inicio, duracao } = this.formRemarcacao.getRawValue();
    this.executar(this.api.reschedule(item.id, new Date(inicio), duracao), 'Sessão remarcada.');
  }

  protected confirmarCancelamento(): void {
    const item = this.cancelando();
    const { quem, motivo } = this.formCancelamento.getRawValue();
    if (!item) return;
    if (!motivo.trim()) {
      this.formCancelamento.markAllAsTouched();
      return;
    }
    this.executar(this.api.cancel(item.id, quem, motivo.trim()), 'Sessão cancelada.');
  }

  protected confirmarEncerramentoSerie(): void {
    const item = this.encerrandoSerie();
    if (!item?.seriesId) return;
    // O último dia da série é a véspera desta sessão, no fuso de quem vê
    const ultimoDia = dateKey(addDays(new Date(item.startsAt), -1));
    this.executar(this.api.endSeries(item.seriesId, ultimoDia), 'Série encerrada.');
  }

  protected confirmar(item: AgendaItem): void {
    this.executar(this.api.confirm(item.id), 'Sessão confirmada.');
  }

  protected registrarRealizada(item: AgendaItem): void {
    this.executar(this.api.complete(item.id), 'Sessão registrada como realizada.');
  }

  protected registrarFalta(item: AgendaItem): void {
    this.executar(this.api.noShow(item.id), 'Falta registrada.');
  }

  // Bastidores

  private executar(chamada: Observable<unknown>, aviso: string): void {
    this.mensagemErro.set(null);
    chamada.subscribe({
      next: () => {
        this.fecharPaineis();
        this.avisos.mostrar(aviso);
        this.carregar();
      },
      error: (erro: HttpErrorResponse) =>
        this.mensagemErro.set(erro.error?.detail ?? 'Não foi possível concluir a ação.'),
    });
  }

  private carregar(): void {
    const id = this.profissionalId();
    const inicio = this.inicioSemana();
    if (!id) return;

    const fim = addDays(inicio, 7);
    this.carregando.set(true);
    forkJoin({
      itens: this.api.getAgenda(id, inicio, fim),
      bloqueios: this.disponibilidadeApi.listBlocks(id, inicio, fim),
    }).subscribe({
      next: ({ itens, bloqueios }) => {
        // Uma resposta atrasada de outra semana não pode sobrescrever a atual
        if (dateKey(this.inicioSemana()) !== dateKey(inicio) || this.profissionalId() !== id) return;
        this.itens.set(itens);
        this.bloqueios.set(bloqueios);
        this.carregando.set(false);
      },
      error: () => {
        this.mensagemErro.set('Não foi possível carregar a agenda.');
        this.carregando.set(false);
      },
    });
  }

  private carregarDisponibilidade(): void {
    const id = this.profissionalId();
    if (!id) return;
    this.periodos.set([]);
    this.disponibilidadeApi.get(id).subscribe({
      next: (d) => {
        this.periodos.set(d.windows);
        this.duracaoPadrao.set(d.defaultSessionMinutes);
      },
      // 404: ainda sem horários configurados; a grade fica sem fundo
      error: () => this.periodos.set([]),
    });
  }

  private temAlgoNoDia(dia: Date): boolean {
    const chave = dateKey(dia);
    return (
      periodosDoDia(this.periodos(), dia).length > 0 ||
      this.itens().some((i) => dateKey(new Date(i.startsAt)) === chave)
    );
  }

  private alertaDeHorario(form: {
    valueChanges: Observable<unknown>;
    controls: { inicio: FormControl<string>; duracao: FormControl<number> };
  }): Signal<string | null> {
    const verificacao = toSignal(
      form.valueChanges.pipe(
        debounceTime(300),
        switchMap(() => {
          const id = this.profissionalId();
          const { inicio, duracao } = { inicio: form.controls.inicio.value, duracao: form.controls.duracao.value };
          if (!id || !inicio || duracao < 15) return of(null);
          return this.disponibilidadeApi.check(id, new Date(inicio), duracao).pipe(catchError(() => of(null)));
        }),
      ),
      { initialValue: null },
    );

    return computed(() => {
      const v = verificacao();
      if (!v) return null;
      if (v.blocked) return 'Este horário cai num bloqueio da agenda (férias, feriado ou compromisso).';
      if (v.configured && !v.withinAvailability)
        return 'Este horário está fora dos horários de atendimento. É possível agendar mesmo assim.';
      return null;
    });
  }
}