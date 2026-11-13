import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { By } from '@angular/platform-browser';
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

  afterEach(() => http.verify());

  function clicar(fixture: ComponentFixture<AgendaPage>, texto: string): void {
    const botoes: HTMLButtonElement[] = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    );
    const botao = botoes.find((b) => b.textContent?.trim() === texto);
    if (!botao) throw new Error(`Botão "${texto}" não encontrado.`);
    botao.click();
    fixture.detectChanges();
  }

  function preencher(fixture: ComponentFixture<AgendaPage>, seletor: string, valor: string): void {
    const campo = fixture.nativeElement.querySelector(seletor) as HTMLInputElement | null;
    if (!campo) throw new Error(`Campo "${seletor}" não encontrado.`);
    campo.value = valor;
    campo.dispatchEvent(new Event('input'));
  }

  function escolher(fixture: ComponentFixture<AgendaPage>, seletor: string, valor: string): void {
    const select = fixture.nativeElement.querySelector(seletor) as HTMLSelectElement | null;
    if (!select) throw new Error(`Seleção "${seletor}" não encontrada.`);
    select.value = valor;
    select.dispatchEvent(new Event('change'));
  }

  it('abre a agenda do primeiro profissional aprovado e mostra a cobrança prevista', async () => {
    const fixture = TestBed.createComponent(AgendaPage);
    fixture.detectChanges();

    http.expectOne('/api/v1/professionals').flush([
      { id: 'p0', professionalName: 'Profissional Pendente', canAttend: false },
      { id: 'p1', professionalName: 'Profissional Teste', canAttend: true },
    ]);

    const req = http.expectOne((r) => r.url === '/api/v1/professionals/p1/agenda');
    const inicioDaSemana = new Date(req.request.params.get('from')!);
    req.flush([
      {
        id: 'a1',
        patientId: 'x',
        patientDisplayName: 'Paciente Teste',
        startsAt: inicioDaSemana.toISOString(),
        endsAt: new Date(inicioDaSemana.getTime() + 50 * 60_000).toISOString(),
        status: 'NoShow',
        chargePercent: 100,
        chargeReason: 'NoShow',
      },
    ]);
    await fixture.whenStable();
    fixture.detectChanges();

    const texto: string = fixture.nativeElement.textContent;
    expect(texto).toContain('Paciente Teste');
    expect(texto).toContain('Cobrança prevista: 100% (falta)');
  });

  it('agenda uma nova sessão enviando o horário em UTC', async () => {
    const fixture = TestBed.createComponent(AgendaPage);
    fixture.detectChanges();

    http.expectOne('/api/v1/professionals').flush([
      { id: 'p1', professionalName: 'Profissional Teste', canAttend: true },
    ]);
    http.expectOne((r) => r.url === '/api/v1/professionals/p1/agenda').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();

    clicar(fixture, 'Nova sessão');
    http.expectOne('/api/v1/professionals/p1/relationships/active').flush([
      { relationshipId: 'v1', patientId: 'x', patientDisplayName: 'Paciente Teste' },
    ]);
    await fixture.whenStable();
    fixture.detectChanges();

    escolher(fixture, '#vinculo', 'v1');
    preencher(fixture, '#inicio', '2026-11-20T15:00');
    fixture.debugElement.query(By.css('form.agendamento')).triggerEventHandler('ngSubmit', null);

    const req = http.expectOne('/api/v1/appointments');
    expect(req.request.body).toEqual({
      relationshipId: 'v1',
      startsAt: new Date('2026-11-20T15:00').toISOString(),
      durationMinutes: 50,
    });

    req.flush({});
    http.expectOne((r) => r.url === '/api/v1/professionals/p1/agenda').flush([]);
  });
});