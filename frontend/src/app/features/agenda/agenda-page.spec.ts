import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AgendaPage } from './agenda-page';

describe('AgendaPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AgendaPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    // As verificações de horário saem sozinhas, 300 ms depois de cada digitação; não são o assunto destes testes
    http.match((r) => r.url.endsWith('/availability/check'));
    http.verify();
  });

  async function abrirAgenda(opcoes: { itens?: (inicioDaSemana: Date) => unknown[]; periodos?: unknown[] } = {}) {
    const fixture = TestBed.createComponent(AgendaPage);
    fixture.detectChanges();

    http.expectOne('/api/v1/professionals').flush([
      { id: 'p0', professionalName: 'Profissional Pendente', canAttend: false },
      { id: 'p1', professionalName: 'Profissional Teste', canAttend: true },
    ]);

    const disponibilidade = http.expectOne('/api/v1/professionals/p1/availability');
    if (opcoes.periodos)
      disponibilidade.flush({
        professionalId: 'p1', timeZoneId: 'America/Sao_Paulo', defaultSessionMinutes: 50,
        breakMinutes: 0, windows: opcoes.periodos, updatedAt: '2026-11-19T12:00:00Z',
      });
    else disponibilidade.flush(null, { status: 404, statusText: 'Not Found' });

    const agenda = http.expectOne((r) => r.url === '/api/v1/professionals/p1/agenda');
    const inicioDaSemana = new Date(agenda.request.params.get('from')!);
    agenda.flush(opcoes.itens?.(inicioDaSemana) ?? []);
    http.expectOne((r) => r.url === '/api/v1/professionals/p1/availability-blocks').flush([]);

    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  async function abrirNovaSessao(fixture: any) {
    clicar(fixture, 'Nova sessão');
    http.expectOne('/api/v1/professionals/p1/relationships/active').flush([
      { relationshipId: 'v1', patientId: 'x', patientDisplayName: 'Paciente Teste' },
    ]);
    await fixture.whenStable();
    fixture.detectChanges();
  }

  function clicar(fixture: any, texto: string): void {
    const botoes: HTMLButtonElement[] = Array.from(fixture.nativeElement.querySelectorAll('button'));
    botoes.find((b) => b.textContent?.trim() === texto)!.click();
    fixture.detectChanges();
  }

  function preencher(fixture: any, seletor: string, valor: string): void {
    const campo: HTMLInputElement = fixture.nativeElement.querySelector(seletor);
    campo.value = valor;
    campo.dispatchEvent(new Event('input'));
  }

  function escolher(fixture: any, seletor: string, valor: string): void {
    const select: HTMLSelectElement = fixture.nativeElement.querySelector(seletor);
    select.value = valor;
    select.dispatchEvent(new Event('change'));
  }

  function marcar(fixture: any, seletor: string): void {
    const caixa: HTMLInputElement = fixture.nativeElement.querySelector(seletor);
    caixa.checked = true;
    caixa.dispatchEvent(new Event('change'));
  }

  async function enviarAgendamento(fixture: any) {
    fixture.debugElement.query(By.css('form.agendamento')).triggerEventHandler('ngSubmit', null);
  }

  it('mostra a sessão na grade e a cobrança prevista nos detalhes', async () => {
    const fixture = await abrirAgenda({
      itens: (inicio) => [{
        id: 'a1', patientId: 'x', patientDisplayName: 'Paciente Teste',
        startsAt: new Date(inicio.getFullYear(), inicio.getMonth(), inicio.getDate(), 10).toISOString(),
        endsAt: new Date(inicio.getFullYear(), inicio.getMonth(), inicio.getDate(), 10, 50).toISOString(),
        status: 'NoShow', chargePercent: 100, chargeReason: 'NoShow', seriesId: null,
      }],
    });

    const cartao: HTMLButtonElement = fixture.nativeElement.querySelector('.cartao');
    expect(cartao.textContent).toContain('Paciente Teste');

    cartao.click();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Cobrança prevista: 100% (falta)');
  });

  it('desenha os períodos de atendimento ao fundo, só nos dias certos', async () => {
    const fixture = await abrirAgenda({ periodos: [{ weekday: 'Monday', start: '09:00:00', end: '12:00:00' }] });

    const colunas = fixture.nativeElement.querySelectorAll('.coluna');
    expect(colunas[0].querySelectorAll('.fundo-atendimento').length).toBe(1);
    expect(colunas[1].querySelectorAll('.fundo-atendimento').length).toBe(0);
  });

  it('agenda uma nova sessão enviando o horário em UTC', async () => {
    const fixture = await abrirAgenda();
    await abrirNovaSessao(fixture);

    escolher(fixture, '#vinculo', 'v1');
    preencher(fixture, '#inicio', '2026-11-20T15:00');
    await enviarAgendamento(fixture);

    const req = http.expectOne('/api/v1/appointments');
    expect(req.request.body).toEqual({
      relationshipId: 'v1',
      startsAt: new Date('2026-11-20T15:00').toISOString(),
      durationMinutes: 50,
    });
    req.flush({});
    http.expectOne((r) => r.url === '/api/v1/professionals/p1/agenda').flush([]);
    http.expectOne((r) => r.url === '/api/v1/professionals/p1/availability-blocks').flush([]);
  });

  it('cria uma série semanal com o dia da semana e o fuso do navegador', async () => {
    const fixture = await abrirAgenda();
    await abrirNovaSessao(fixture);

    escolher(fixture, '#vinculo', 'v1');
    preencher(fixture, '#inicio', '2026-11-17T15:00');
    marcar(fixture, '#repetir');
    await enviarAgendamento(fixture);

    const req = http.expectOne('/api/v1/series');
    expect(req.request.body).toEqual({
      relationshipId: 'v1',
      weekday: 'Tuesday',
      localStartTime: '15:00:00',
      startsOn: '2026-11-17',
      durationMinutes: 50,
      timeZoneId: Intl.DateTimeFormat().resolvedOptions().timeZone,
    });
    req.flush({});
    http.expectOne((r) => r.url === '/api/v1/professionals/p1/agenda').flush([]);
    http.expectOne((r) => r.url === '/api/v1/professionals/p1/availability-blocks').flush([]);
  });

  it('avisa, enquanto se escolhe o horário, que ele está fora do atendimento', async () => {
    const fixture = await abrirAgenda({ periodos: [{ weekday: 'Monday', start: '09:00:00', end: '12:00:00' }] });
    await abrirNovaSessao(fixture);

    preencher(fixture, '#inicio', '2026-11-21T10:00'); // um sábado
    await new Promise((resolve) => setTimeout(resolve, 350)); // espera o debounce de 300 ms

    const verificacoes = http.match((r) => r.url === '/api/v1/professionals/p1/availability/check');
    verificacoes.at(-1)!.flush({ configured: true, withinAvailability: false, blocked: false });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('fora dos horários de atendimento');
  });
});