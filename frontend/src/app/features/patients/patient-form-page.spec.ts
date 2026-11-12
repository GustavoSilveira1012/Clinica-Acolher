import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { PatientFormPage } from './patient-form-page';

describe('PatientFormPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [PatientFormPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function preencher(fixture: any, id: string, valor: string): void {
    const input: HTMLInputElement = fixture.nativeElement.querySelector(`#${id}`);
    input.value = valor;
    input.dispatchEvent(new Event('input'));
  }

  function enviar(fixture: any): void {
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);
    fixture.detectChanges();
  }

  it('não envia nada à API com campos obrigatórios vazios', () => {
    const fixture = TestBed.createComponent(PatientFormPage);
    fixture.detectChanges();

    enviar(fixture);

    expect(fixture.nativeElement.textContent).toContain('Informe o nome completo');
  });

  it('envia campos opcionais vazios como null', () => {
    const fixture = TestBed.createComponent(PatientFormPage);
    fixture.detectChanges();

    preencher(fixture, 'fullName', '  Paciente Teste  ');
    preencher(fixture, 'birthDate', '1990-05-20');
    enviar(fixture);

    const req = http.expectOne('/api/v1/patients');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      fullName: 'Paciente Teste',
      birthDate: '1990-05-20',
      socialName: null,
      email: null,
      phone: null,
    });
  });
});