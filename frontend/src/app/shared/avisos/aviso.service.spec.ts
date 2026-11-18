import { TestBed } from '@angular/core/testing';
import { AvisoService } from './aviso.service';

describe('AvisoService', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it('mostra o aviso e o retira sozinho depois de alguns segundos', () => {
    const servico = TestBed.inject(AvisoService);

    servico.mostrar('Sessão cancelada.');
    expect(servico.avisos().map((a) => a.texto)).toEqual(['Sessão cancelada.']);

    vi.advanceTimersByTime(AvisoService.DURACAO_MS);
    expect(servico.avisos()).toEqual([]);
  });

  it('fechar retira só o aviso escolhido', () => {
    const servico = TestBed.inject(AvisoService);
    servico.mostrar('Primeiro.');
    servico.mostrar('Segundo.');

    servico.fechar(servico.avisos()[0].id);

    expect(servico.avisos().map((a) => a.texto)).toEqual(['Segundo.']);
  });
});

