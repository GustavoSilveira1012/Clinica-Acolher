import { AvailabilityBlock, AvailabilityWindow } from '../availability/availability.models';
import { AgendaItem } from './agenda.models';
import { addDays, weekdayOf } from './week';

export const PIXELS_POR_HORA = 56;

export interface Faixa {
  inicio: number; // hora inteira
  fim: number;
}

export interface Posicao {
  topo: number;
  altura: number;
}

export function minutosDeTexto(hora: string): number {
  const [h, m] = hora.split(':').map(Number);
  return h * 60 + m;
}

export function minutosDoDia(data: Date): number {
  return data.getHours() * 60 + data.getMinutes();
}

/** As horas da grade: no mínimo das 8h às 18h, esticadas para caber todos os períodos e sessões. */
export function faixaDeHoras(periodos: AvailabilityWindow[], itens: AgendaItem[]): Faixa {
  let inicio = 8;
  let fim = 18;
  for (const p of periodos) {
    inicio = Math.min(inicio, Math.floor(minutosDeTexto(p.start) / 60));
    fim = Math.max(fim, Math.ceil(minutosDeTexto(p.end) / 60));
  }
  for (const item of itens) {
    inicio = Math.min(inicio, new Date(item.startsAt).getHours());
    fim = Math.max(fim, Math.ceil(minutosDoDia(new Date(item.endsAt)) / 60));
  }
  return { inicio, fim: Math.min(fim, 24) };
}

/** Topo e altura, em pixels, de algo que vai de um minuto do dia até outro. */
export function posicao(inicioMin: number, fimMin: number, faixa: Faixa): Posicao {
  const pixelsPorMinuto = PIXELS_POR_HORA / 60;
  return {
    topo: (inicioMin - faixa.inicio * 60) * pixelsPorMinuto,
    altura: Math.max((fimMin - inicioMin) * pixelsPorMinuto, 20),
  };
}

export function periodosDoDia(periodos: AvailabilityWindow[], dia: Date): AvailabilityWindow[] {
  const nome = weekdayOf(dia);
  return periodos.filter((p) => p.weekday === nome);
}

export interface TrechoDeBloqueio {
  inicioMin: number;
  fimMin: number;
  motivo: string | null;
}

/** Recorta cada bloqueio ao dia mostrado: umas férias atravessam vários dias. */
export function bloqueiosDoDia(bloqueios: AvailabilityBlock[], dia: Date): TrechoDeBloqueio[] {
  const comeco = dia.getTime();
  const fim = addDays(dia, 1).getTime();
  return bloqueios
    .map((b) => ({
      de: Math.max(new Date(b.startsAt).getTime(), comeco),
      ate: Math.min(new Date(b.endsAt).getTime(), fim),
      motivo: b.reason,
    }))
    .filter((t) => t.de < t.ate)
    .map((t) => ({ inicioMin: (t.de - comeco) / 60_000, fimMin: (t.ate - comeco) / 60_000, motivo: t.motivo }));
}