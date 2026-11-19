import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { AvisoService } from '../../shared/avisos/aviso.service';
import { Dialogo } from '../../shared/dialogo/dialogo';
import { Weekday, addDays, fromDateKey } from '../agenda/week';
import { AvailabilityApi } from './availability-api';
import { Availability, AvailabilityBlock } from './availability.models';

type PeriodoForm = FormGroup<{
  dia: FormControl<Weekday>;
  inicio: FormControl<string>;
  fim: FormControl<string>;
}>;

const DIAS: { valor: Weekday; nome: string }[] = [
  { valor: 'Monday', nome: 'Segunda-feira' },
  { valor: 'Tuesday', nome: 'Terça-feira' },
  { valor: 'Wednesday', nome: 'Quarta-feira' },
  { valor: 'Thursday', nome: 'Quinta-feira' },
  { valor: 'Friday', nome: 'Sexta-feira' },
  { valor: 'Saturday', nome: 'Sábado' },
  { valor: 'Sunday', nome: 'Domingo' },
];

const DIAS_UTEIS_APOS_SEGUNDA: Weekday[] = ['Tuesday', 'Wednesday', 'Thursday', 'Friday'];

@Component({
  selector: 'app-availability-page',
  imports: [DatePipe, ReactiveFormsModule, Dialogo],
  templateUrl: './availability-page.html',
  styleUrl: './availability-page.css',
})
export class AvailabilityPage implements OnInit {
  private readonly api = inject(AvailabilityApi);
  private readonly auth = inject(AuthService);
  private readonly avisos = inject(AvisoService);

  private profissionalId = '';

  protected readonly dias = DIAS;
  protected readonly fuso = signal(Intl.DateTimeFormat().resolvedOptions().timeZone);
  protected readonly carregado = signal(false);
  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);

  protected readonly form = new FormGroup({
    duracao: new FormControl(50, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(15), Validators.max(180)],
    }),
    intervalo: new FormControl(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0), Validators.max(60)],
    }),
    periodos: new FormArray<PeriodoForm>([]),
  });

  protected readonly bloqueios = signal<AvailabilityBlock[]>([]);
  protected readonly sessoesAfetadas = signal(0);
  protected readonly removendo = signal<AvailabilityBlock | null>(null);
  protected readonly erroBloqueio = signal<string | null>(null);

  protected readonly formBloqueio = new FormGroup({
    inicio: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    fim: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    motivo: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(120)] }),
  });

  ngOnInit(): void {
    const id = this.auth.user()?.professionalId;
    if (!id) {
      this.erro.set('Os horários de atendimento são configurados por cada profissional.');
      return;
    }
    this.profissionalId = id;

    this.api.get(id).subscribe({
      next: (disponibilidade) => {
        this.preencher(disponibilidade);
        this.carregado.set(true);
      },
      // 404: ainda não configurou; a tela começa vazia
      error: (erro: HttpErrorResponse) =>
        erro.status === 404 ? this.carregado.set(true) : this.erro.set(this.traduzir(erro)),
    });
    this.carregarBloqueios();
  }

  protected periodosDoDia(dia: Weekday): { controle: PeriodoForm; indice: number }[] {
    return this.form.controls.periodos.controls
      .map((controle, indice) => ({ controle, indice }))
      .filter((p) => p.controle.controls.dia.value === dia);
  }

  protected adicionarPeriodo(dia: Weekday): void {
    const jaTemPeriodo = this.periodosDoDia(dia).length > 0;
    this.form.controls.periodos.push(
      jaTemPeriodo ? this.novoPeriodo(dia, '14:00', '18:00') : this.novoPeriodo(dia, '09:00', '12:00'),
    );
  }

  protected removerPeriodo(indice: number): void {
    this.form.controls.periodos.removeAt(indice);
  }

  protected copiarSegunda(): void {
    const periodos = this.form.controls.periodos;
    const daSegunda = this.periodosDoDia('Monday').map((p) => p.controle.getRawValue());

    // Remove de trás para frente, para os índices não mudarem no meio do caminho
    for (let i = periodos.length - 1; i >= 0; i--)
      if (DIAS_UTEIS_APOS_SEGUNDA.includes(periodos.at(i).controls.dia.value)) periodos.removeAt(i);

    for (const dia of DIAS_UTEIS_APOS_SEGUNDA)
      for (const p of daSegunda) periodos.push(this.novoPeriodo(dia, p.inicio, p.fim));
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { duracao, intervalo, periodos } = this.form.getRawValue();

    this.salvando.set(true);
    this.erro.set(null);
    this.api
      .save(this.profissionalId, {
        timeZoneId: this.fuso(),
        defaultSessionMinutes: duracao,
        breakMinutes: intervalo,
        windows: periodos.map((p) => ({ weekday: p.dia, start: `${p.inicio}:00`, end: `${p.fim}:00` })),
      })
      .subscribe({
        next: (disponibilidade) => {
          this.salvando.set(false);
          this.preencher(disponibilidade);
          this.avisos.mostrar('Horários salvos.');
        },
        error: (erro: HttpErrorResponse) => {
          this.salvando.set(false);
          this.erro.set(this.traduzir(erro));
        },
      });
  }

  protected criarBloqueio(): void {
    if (this.formBloqueio.invalid) {
      this.formBloqueio.markAllAsTouched();
      return;
    }
    const { inicio, fim, motivo } = this.formBloqueio.getRawValue();
    const comeco = fromDateKey(inicio);
    const termino = addDays(fromDateKey(fim), 1); // dias inteiros: até a meia-noite seguinte ao último dia

    if (termino <= comeco) {
      this.erroBloqueio.set('A data final precisa ser igual ou posterior à inicial.');
      return;
    }

    this.erroBloqueio.set(null);
    this.api.createBlock(this.profissionalId, comeco, termino, motivo.trim() || null).subscribe({
      next: (criado) => {
        this.formBloqueio.reset();
        this.sessoesAfetadas.set(criado.affectedAppointments);
        this.avisos.mostrar('Bloqueio criado.');
        this.carregarBloqueios();
      },
      error: (erro: HttpErrorResponse) => this.erroBloqueio.set(this.traduzir(erro)),
    });
  }

  protected confirmarRemocao(): void {
    const bloqueio = this.removendo();
    if (!bloqueio) return;
    this.api.deleteBlock(bloqueio.id).subscribe({
      next: () => {
        this.removendo.set(null);
        this.avisos.mostrar('Bloqueio removido.');
        this.carregarBloqueios();
      },
      error: (erro: HttpErrorResponse) => {
        this.removendo.set(null);
        this.erroBloqueio.set(this.traduzir(erro));
      },
    });
  }

  /** O fim do bloqueio é a meia-noite seguinte; o último dia é a véspera. */
  protected ultimoDia(bloqueio: AvailabilityBlock): Date {
    return new Date(new Date(bloqueio.endsAt).getTime() - 1);
  }

  private preencher(d: Availability): void {
    this.fuso.set(d.timeZoneId);
    this.form.patchValue({ duracao: d.defaultSessionMinutes, intervalo: d.breakMinutes });
    this.form.controls.periodos.clear();
    for (const w of d.windows)
      this.form.controls.periodos.push(this.novoPeriodo(w.weekday, w.start.slice(0, 5), w.end.slice(0, 5)));
  }

  private novoPeriodo(dia: Weekday, inicio: string, fim: string): PeriodoForm {
    return new FormGroup({
      dia: new FormControl(dia, { nonNullable: true }),
      inicio: new FormControl(inicio, { nonNullable: true, validators: [Validators.required] }),
      fim: new FormControl(fim, { nonNullable: true, validators: [Validators.required] }),
    });
  }

  private carregarBloqueios(): void {
    if (!this.profissionalId) return;
    const hoje = addDays(new Date(), 0);
    this.api.listBlocks(this.profissionalId, hoje, addDays(hoje, 366)).subscribe({
      next: (bloqueios) => this.bloqueios.set(bloqueios),
    });
  }

  private traduzir(erro: HttpErrorResponse): string {
    return erro.error?.detail ?? 'Não foi possível concluir a ação. Tente de novo.';
  }
}