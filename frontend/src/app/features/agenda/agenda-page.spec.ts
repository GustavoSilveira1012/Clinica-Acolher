import { TestBed } from '@angular/core/testing';
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

  afterEach(() => http.verify());

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
});