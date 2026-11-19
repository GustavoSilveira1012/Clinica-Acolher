import { AvailabilityWindow } from '../availability/availability.models';
import { AgendaItem } from './agenda.models';
import { bloqueiosDoDia, faixaDeHoras, periodosDoDia, posicao } from './grade';

function item(inicio: Date, fim: Date): AgendaItem {
  return {
    id: 'a1', patientId: 'x', patientDisplayName: 'Paciente Teste',
    startsAt: inicio.toISOString(), endsAt: fim.toISOString(),
    status: 'Scheduled', chargePercent: 0, chargeReason: 'None', seriesId: null,
  };
}

describe('grade da agenda', () => {
  const manha: AvailabilityWindow[] = [{ weekday: 'Monday', start: '09:00:00', end: '12:00:00' }];

  it('estica a faixa de horas para caber uma sessão à noite', () => {
    const noite = item(new Date(2026, 10, 16, 20, 0), new Date(2026, 10, 16, 20, 50));

    expect(faixaDeHoras(manha, [noite])).toEqual({ inicio: 8, fim: 21 });
  });

  it('converte minutos em pixels a partir do início da faixa', () => {
    expect(posicao(9 * 60, 10 * 60, { inicio: 8, fim: 18 })).toEqual({ topo: 56, altura: 56 });
  });

  it('recorta um bloqueio de vários dias para cada dia mostrado', () => {
    const ferias = {
      id: 'b1', reason: 'Férias',
      startsAt: new Date(2026, 11, 21).toISOString(),
      endsAt: new Date(2026, 11, 24).toISOString(),
    };

    expect(bloqueiosDoDia([ferias], new Date(2026, 11, 22))).toEqual([{ inicioMin: 0, fimMin: 24 * 60, motivo: 'Férias' }]);
    expect(bloqueiosDoDia([ferias], new Date(2026, 11, 24))).toEqual([]);
  });

  it('pega só os períodos do dia da semana mostrado', () => {
    const semana: AvailabilityWindow[] = [...manha, { weekday: 'Tuesday', start: '14:00:00', end: '18:00:00' }];

    expect(periodosDoDia(semana, new Date(2026, 10, 16)).map((p) => p.weekday)).toEqual(['Monday']);
  });
});