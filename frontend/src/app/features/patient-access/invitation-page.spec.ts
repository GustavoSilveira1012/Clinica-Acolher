import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { InvitationPage } from './invitation-page';

describe('InvitationPage', () => {
  let http: HttpTestingController;

  function configurar(fragmento: string | null) {
    TestBed.configureTestingModule({
      imports: [InvitationPage],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { fragment: fragmento } } },
      ],
    });
    http = TestBed.inject(HttpTestingController);
  }

  afterEach(() => http.verify());

  function preencher(fixture: any, seletor: string, valor: string) {
    const campo: HTMLInputElement = fixture.nativeElement.querySelector(seletor);
    campo.value = valor;
    campo.dispatchEvent(new Event('input'));
  }

  it('envia o código do fragmento e tira o código da barra de endereços', () => {
    configurar('codigo-de-teste');
    const substituir = vi.spyOn(history, 'replaceState');
    const fixture = TestBed.createComponent(InvitationPage);
    fixture.detectChanges();

    preencher(fixture, '#senha', 'minha frase secreta');
    preencher(fixture, '#confirmacao', 'minha frase secreta');
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);

    expect(substituir).toHaveBeenCalled();
    const req = http.expectOne('/api/v1/patient-invitations/accept');
    expect(req.request.body).toEqual({ token: 'codigo-de-teste', password: 'minha frase secreta' });
  });

  it('não envia nada quando as senhas não conferem', () => {
    configurar('codigo-de-teste');
    const fixture = TestBed.createComponent(InvitationPage);
    fixture.detectChanges();

    preencher(fixture, '#senha', 'minha frase secreta');
    preencher(fixture, '#confirmacao', 'outra frase secreta');
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('As senhas não conferem');
  });

  it('avisa quando o link chega sem o código', () => {
    configurar(null);
    const fixture = TestBed.createComponent(InvitationPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Este link está incompleto');
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
  });
});