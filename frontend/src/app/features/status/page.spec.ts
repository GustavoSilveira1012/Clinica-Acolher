import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { StatusPage } from './status-page/status-page';

describe('StatusPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [StatusPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('mostra sucesso quando a API responde', async () => {
    const fixture = TestBed.createComponent(StatusPage);
    fixture.detectChanges();

    http.expectOne('/api/health').flush({
      status: 'ok',
      service: 'ClinicaAcolher.Api',
      timestamp: '2026-11-09T18:00:00Z',
    });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('API funcionando');
  });

  it('mostra erro quando a API falha', async () => {
    const fixture = TestBed.createComponent(StatusPage);
    fixture.detectChanges();

    http.expectOne('/api/health').flush('falhou', { status: 500, statusText: 'Erro' });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Não foi possível conectar à API.');
  });
});
