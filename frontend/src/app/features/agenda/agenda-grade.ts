import { Component, computed, input, output } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AvailabilityBlock, AvailabilityWindow } from '../availability/availability.models';
import { AgendaItem, ROTULO_STATUS } from './agenda.models';
import { PIXELS_POR_HORA, bloqueiosDoDia, faixaDeHoras, minutosDeTexto, minutosDoDia, periodosDoDia, posicao } from './grade';
import { dateKey } from './week';

@Component({
  selector: 'app-agenda-grade',
  imports: [DatePipe],
  templateUrl: './agenda-grade.html',
  styleUrl: './agenda-grade.css',
})
export class AgendaGrade {
  readonly dias = input.required<Date[]>();
  readonly itens = input.required<AgendaItem[]>();
  readonly periodos = input<AvailabilityWindow[]>([]);
  readonly bloqueios = input<AvailabilityBlock[]>([]);
  readonly selecionar = output<AgendaItem>();

  protected readonly pixelsPorHora = PIXELS_POR_HORA;
  protected readonly faixa = computed(() => faixaDeHoras(this.periodos(), this.itens()));
  protected readonly horas = computed(() => {
    const { inicio, fim } = this.faixa();
    return Array.from({ length: fim - inicio }, (_, i) => inicio + i);
  });
  protected readonly altura = computed(() => this.horas().length * PIXELS_POR_HORA);

  protected readonly colunas = computed(() => {
    const faixa = this.faixa();
    const hoje = dateKey(new Date());

    return this.dias().map((dia) => {
      const chave = dateKey(dia);
      const bloqueios = bloqueiosDoDia(this.bloqueios(), dia);

      return {
        dia,
        chave,
        hoje: chave === hoje,
        bloqueado: bloqueios.length > 0,
        periodos: periodosDoDia(this.periodos(), dia).map((p) =>
          posicao(minutosDeTexto(p.start), minutosDeTexto(p.end), faixa),
        ),
        bloqueios: bloqueios.map((b) => ({ ...posicao(b.inicioMin, b.fimMin, faixa), motivo: b.motivo })),
        sessoes: this.itens()
          .filter((item) => dateKey(new Date(item.startsAt)) === chave)
          .map((item) => ({
            item,
            ...posicao(minutosDoDia(new Date(item.startsAt)), minutosDoDia(new Date(item.endsAt)) || 24 * 60, faixa),
          })),
      };
    });
  });

  protected rotuloStatus(item: AgendaItem): string {
    return ROTULO_STATUS[item.status];
  }
}