import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PatientListPage } from './patient-list-page';

describe('PatientListPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PatientListPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('busca pacientes ativos ao abrir e mostra a lista', async () => {
    const fixture = TestBed.createComponent(PatientListPage);
    fixture.detectChanges();

    const req = http.expectOne('/api/v1/patients/search');
    expect(req.request.body).toEqual({ search: null, active: true, page: 1, pageSize: 20 });
    req.flush({
      items: [{ id: '1', displayName: 'Paciente Teste', email: null, phone: null, active: true }],
      page: 1,
      pageSize: 20,
      totalCount: 1,
    });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Paciente Teste');
  });

  it('avisa quando nenhum paciente é encontrado', async () => {
    const fixture = TestBed.createComponent(PatientListPage);
    fixture.detectChanges();

    http.expectOne('/api/v1/patients/search').flush({ items: [], page: 1, pageSize: 20, totalCount: 0 });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Nenhum paciente encontrado');
  });
});