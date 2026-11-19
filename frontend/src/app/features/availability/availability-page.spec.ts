import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { fromDateKey } from '../agenda/week';
import { AvailabilityPage } from './availability-page';

describe('AvailabilityPage', () => {
  let http: HttpTestingController;

  const disponibilidade = {
    professionalId: 'p1',
    timeZoneId: 'America/Sao_Paulo',
    defaultSessionMinutes: 50,
    breakMinutes: 10,
    windows: [
      { weekday: 'Monday', start: '09:00:00', end: '12:00:00' },
      { weekday: 'Monday', start: '14:00:00', end: '18:00:00' },
    ],
    updatedAt: '2026-11-18T12:00:00Z',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [AvailabilityPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);

    // Uma profissional logada
    TestBed.inject(AuthService).loadSession().subscribe();
    http.expectOne('/api/v1/auth/me').flush({
      id: 'u1', email: 'profissional@exemplo.org', roles: ['Professional'],
      professionalId: 'p1', patientId: null, twoFactorEnabled: true, mustSetupTwoFactor: false,
    });
  });

  afterEach(() => http.verify());

  async function abrir() {
    const fixture = TestBed.createComponent(AvailabilityPage);
    fixture.detectChanges();
    http.expectOne('/api/v1/professionals/p1/availability').flush(disponibilidade);
    http.expectOne((r) => r.url === '/api/v1/professionals/p1/availability-blocks').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  function botao(fixture: any, texto: string): HTMLButtonElement {
    return Array.from<HTMLButtonElement>(fixture.nativeElement.querySelectorAll('button'))
      .find((b) => b.textContent?.trim() === texto)!;
  }

  it('mostra os períodos salvos agrupados no dia certo', async () => {
    const fixture = await abrir();

    const segunda: HTMLFieldSetElement = fixture.nativeElement.querySelectorAll('fieldset')[0];
    expect(segunda.querySelector('legend')!.textContent).toContain('Segunda');
    expect(segunda.querySelectorAll('.periodo').length).toBe(2);
  });

  it('copia a segunda para os dias úteis e salva com os segundos e o fuso', async () => {
    const fixture = await abrir();

    botao(fixture, 'Copiar segunda para terça a sexta').click();
    botao(fixture, 'Salvar').click();

    const req = http.expectOne('/api/v1/professionals/p1/availability');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body.timeZoneId).toBe('America/Sao_Paulo');
    expect(req.request.body.windows.length).toBe(10);
    expect(req.request.body.windows).toContainEqual({ weekday: 'Friday', start: '14:00:00', end: '18:00:00' });
  });

  it('avisa quantas sessões já marcadas caem no bloqueio', async () => {
    const fixture = await abrir();

    const preencher = (seletor: string, valor: string) => {
      const campo: HTMLInputElement = fixture.nativeElement.querySelector(seletor);
      campo.value = valor;
      campo.dispatchEvent(new Event('input'));
    };
    preencher('#bloqueio-inicio', '2026-12-21');
    preencher('#bloqueio-fim', '2027-01-03');
    botao(fixture, 'Bloquear').click();

    const req = http.expectOne('/api/v1/professionals/p1/availability-blocks');
    expect(req.request.body.startsAt).toBe(fromDateKey('2026-12-21').toISOString());
    expect(req.request.body.endsAt).toBe(fromDateKey('2027-01-04').toISOString());

    req.flush({ block: { id: 'b1', startsAt: '', endsAt: '', reason: null }, affectedAppointments: 2 });
    http.expectOne((r) => r.url === '/api/v1/professionals/p1/availability-blocks').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('2 sessões já marcadas caem');
  });
});
