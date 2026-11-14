/** Segunda-feira, à meia-noite local, da semana que contém a data. */
export function startOfWeek(date: Date): Date {
  const diasDesdeSegunda = (date.getDay() + 6) % 7;
  return new Date(date.getFullYear(), date.getMonth(), date.getDate() - diasDesdeSegunda);
}

/** A data à meia-noite local, somada de alguns dias. */
export function addDays(date: Date, days: number): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate() + days);
}

/** Chave "aaaa-mm-dd" no fuso local, para agrupar as sessões por dia. */
export function dateKey(date: Date): string {
  const mes = String(date.getMonth() + 1).padStart(2, '0');
  const dia = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${mes}-${dia}`;
}

/** Valor para <input type="datetime-local">, no fuso local: "aaaa-mm-ddThh:mm". */
export function toLocalInputValue(date: Date): string {
  const hora = String(date.getHours()).padStart(2, '0');
  const minuto = String(date.getMinutes()).padStart(2, '0');
  return `${dateKey(date)}T${hora}:${minuto}`;
}

/** A próxima hora cheia depois de um momento. */
export function nextFullHour(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate(), date.getHours() + 1);
}

const DIAS_DA_SEMANA = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'] as const;
export type Weekday = (typeof DIAS_DA_SEMANA)[number];

/** O dia da semana, a hora local e a data de início de uma série, a partir de um horário local. */
export function toSeriesSchedule(date: Date): { weekday: Weekday; localStartTime: string; startsOn: string } {
  const hora = String(date.getHours()).padStart(2, '0');
  const minuto = String(date.getMinutes()).padStart(2, '0');
  return {
    weekday: DIAS_DA_SEMANA[date.getDay()],
    localStartTime: `${hora}:${minuto}:00`,
    startsOn: dateKey(date),
  };
}