import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AvisoService } from '../../shared/avisos/aviso.service';
import { PortalPage } from './portal-page';
import { PortalAgenda, PortalPolicy } from './portal.models';

describe('PortalPage', () => {
  let http: HttpTestingController;
  const politica: PortalPolicy = {
    policyId: 'politica-1', professionalName: 'Profissional Teste', version: 1,
    minimumNoticeHours: 24, lateCancellationChargePercent: 100, noShowChargePercent: 100,
    publishedAt: '2026-11-20T12:00:00Z', accepted: false, acceptedAt: null,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PortalPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function carregar(politicas: PortalPolicy[], agenda: PortalAgenda = { upcoming: [], recent: [] }) {
    const fixture = TestBed.createComponent(PortalPage);
    fixture.detectChanges();
    http.expectOne('/api/v1/portal/appointments').flush(agenda);
    http.expectOne('/api/v1/portal/attendance-policies').flush(politicas);
    fixture.detectChanges();
    return fixture;
  }

  it('exige marcar concordância e mostra a confirmação e a data após o aceite', () => {
    const fixture = carregar([politica]);
    const botao: HTMLButtonElement = fixture.nativeElement.querySelector('.politica button');
    expect(botao.disabled).toBe(true);
    botao.click();
    http.expectNone('/api/v1/portal/attendance-policies/politica-1/accept');

    fixture.nativeElement.querySelector('input[type=checkbox]').click();
    fixture.detectChanges();
    expect(botao.disabled).toBe(false);
    botao.click();
    const aceite = http.expectOne('/api/v1/portal/attendance-policies/politica-1/accept');
    expect(aceite.request.method).toBe('POST');
    aceite.flush({ ...politica, accepted: true, acceptedAt: '2026-11-23T12:00:00Z' });
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.politica')).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Você aceitou');
    expect(fixture.nativeElement.textContent).toContain('23/11/2026');
    expect(TestBed.inject(AvisoService).avisos().some(a => a.texto === 'Política aceita.')).toBe(true);
  });

  it('mostra o profissional nas próximas sessões e mantém o histórico recolhido', () => {
    const fixture = carregar([], {
      upcoming: [{ id: 'futura', startsAt: '2026-11-30T15:00:00Z', endsAt: '2026-11-30T15:50:00Z',
        status: 'Scheduled', professionalName: 'Profissional Futuro Teste' }],
      recent: [{ id: 'passada', startsAt: '2026-11-16T15:00:00Z', endsAt: '2026-11-16T15:50:00Z',
        status: 'Completed', professionalName: 'Profissional Histórico Teste' }],
    });
    const historico: HTMLDetailsElement = fixture.nativeElement.querySelector('details');
    expect(historico.open).toBe(false);
    expect(historico.querySelector('summary')!.textContent).toContain('Sessões dos últimos 90 dias (1)');
    expect(historico.textContent).toContain('Profissional Histórico Teste');
    expect(fixture.nativeElement.querySelector('[aria-labelledby=proximas-titulo]').textContent)
      .toContain('Profissional Futuro Teste');
    historico.querySelector('summary')!.click();
    expect(historico.open).toBe(true);
  });

  it('ao recarregar com a versão 2 pendente exige nova concordância', () => {
    const anterior = carregar([{ ...politica, accepted: true, acceptedAt: '2026-11-23T12:00:00Z' }]);
    expect(anterior.nativeElement.textContent).toContain('Você aceitou');
    anterior.destroy();

    const atual = carregar([{ ...politica, policyId: 'politica-2', version: 2 }]);
    expect(atual.nativeElement.querySelector('.politica').textContent).toContain('versão 2');
    expect(atual.nativeElement.querySelector('input[type=checkbox]').checked).toBe(false);
    expect(atual.nativeElement.querySelector('.politica button').disabled).toBe(true);
  });
});
