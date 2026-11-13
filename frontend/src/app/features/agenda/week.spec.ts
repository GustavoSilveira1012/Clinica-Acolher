import { addDays, dateKey, nextFullHour, startOfWeek, toLocalInputValue } from './week';

describe('week', () => {
  // Atenção: no construtor de Date, o mês começa em 0. new Date(2026, 10, 18) é 18 de NOVEMBRO
  it('uma quarta-feira pertence à semana que começa na segunda anterior', () => {
    expect(dateKey(startOfWeek(new Date(2026, 10, 18)))).toBe('2026-11-16');
  });

  it('um domingo pertence à semana que começou seis dias antes', () => {
    expect(dateKey(startOfWeek(new Date(2026, 10, 22)))).toBe('2026-11-16');
  });

  it('uma segunda-feira é o início da própria semana', () => {
    expect(dateKey(startOfWeek(new Date(2026, 10, 16, 14, 30)))).toBe('2026-11-16');
  });

  it('atravessa a virada do mês', () => {
    expect(dateKey(startOfWeek(new Date(2026, 11, 1)))).toBe('2026-11-30');
    expect(dateKey(addDays(new Date(2026, 10, 30), 1))).toBe('2026-12-01');
  });

  it('converte para o campo de data e hora e volta para a mesma hora local', () => {
    const valor = toLocalInputValue(new Date(2026, 10, 20, 9, 5));

    expect(valor).toBe('2026-11-20T09:05');
    expect(new Date(valor).getHours()).toBe(9);
  });

  it('a próxima hora cheia vira o dia quando necessário', () => {
    expect(toLocalInputValue(nextFullHour(new Date(2026, 10, 20, 23, 40)))).toBe('2026-11-21T00:00');
  });
});