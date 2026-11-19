import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { LoginPage } from './login-page';
import { AuthService } from '../../core/auth/auth.service';

@Component({ template: '' })
class TestRoute {}

describe('LoginPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: 'agenda', component: TestRoute }]),
      ],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('mostra a mensagem da API e apaga a senha quando o login falha', async () => {
    const fixture = TestBed.createComponent(LoginPage);
    fixture.detectChanges();

    const email: HTMLInputElement = fixture.nativeElement.querySelector('#email');
    const senha: HTMLInputElement = fixture.nativeElement.querySelector('#senha');
    email.value = 'profissional@exemplo.org';
    email.dispatchEvent(new Event('input'));
    senha.value = 'senha-errada-123';
    senha.dispatchEvent(new Event('input'));
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);

    http.expectOne('/api/v1/auth/login').flush(
      { detail: 'E-mail ou senha incorretos, ou acesso bloqueado temporariamente por muitas tentativas.' },
      { status: 401, statusText: 'Unauthorized' },
    );
    await fixture.whenStable();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('E-mail ou senha incorretos');
    expect(senha.value).toBe('');
  });

  it('pede o código autenticador depois da senha e conclui a segunda etapa', async () => {
    const fixture = TestBed.createComponent(LoginPage);
    fixture.detectChanges();

    const email: HTMLInputElement = fixture.nativeElement.querySelector('#email');
    const senha: HTMLInputElement = fixture.nativeElement.querySelector('#senha');
    email.value = 'profissional@exemplo.org';
    email.dispatchEvent(new Event('input'));
    senha.value = 'senha-correta-123';
    senha.dispatchEvent(new Event('input'));
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);

    http.expectOne('/api/v1/auth/login').flush({ requiresTwoFactor: true, user: null });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('#codigo')).not.toBeNull();

    const codigo: HTMLInputElement = fixture.nativeElement.querySelector('#codigo');
    codigo.value = '123456';
    codigo.dispatchEvent(new Event('input'));
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);

    const request = http.expectOne('/api/v1/auth/login/2fa');
    expect(request.request.body).toEqual({ code: '123456' });
    request.flush({
      requiresTwoFactor: false,
      user: {
        id: 'u1',
        email: 'profissional@exemplo.org',
        roles: ['Professional'],
        professionalId: 'p1',
        patientId: null,
        twoFactorEnabled: true,
        mustSetupTwoFactor: false,
      },
    });

    await fixture.whenStable();
    expect(TestBed.inject(AuthService).isLoggedIn()).toBe(true);
  });

  it('pede o código do aplicativo quando a conta tem 2FA', async () => {
    const fixture = TestBed.createComponent(LoginPage);
    fixture.detectChanges();

    const preencher = (seletor: string, valor: string) => {
      const campo: HTMLInputElement = fixture.nativeElement.querySelector(seletor);
      campo.value = valor;
      campo.dispatchEvent(new Event('input'));
    };

    preencher('#email', 'profissional@exemplo.org');
    preencher('#senha', 'senha-correta-123');
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);

    http.expectOne('/api/v1/auth/login').flush({ requiresTwoFactor: true, user: null });
    await fixture.whenStable();
    fixture.detectChanges();

    preencher('#codigo', '123456');
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);

    const request = http.expectOne('/api/v1/auth/login/2fa');
    expect(request.request.body).toEqual({ code: '123456' });
  });

  it('usa um código de recuperação e apaga a senha ao voltar', async () => {
    const fixture = TestBed.createComponent(LoginPage);
    fixture.detectChanges();

    const email: HTMLInputElement = fixture.nativeElement.querySelector('#email');
    const senha: HTMLInputElement = fixture.nativeElement.querySelector('#senha');
    email.value = 'profissional@exemplo.org';
    email.dispatchEvent(new Event('input'));
    senha.value = 'senha-correta-123';
    senha.dispatchEvent(new Event('input'));
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);

    http.expectOne('/api/v1/auth/login').flush({ requiresTwoFactor: true, user: null });
    await fixture.whenStable();
    fixture.detectChanges();

    const buttons: HTMLButtonElement[] = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    );
    buttons.find((button) => button.textContent?.includes('código de recuperação'))!.click();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Código de recuperação');

    const codigo: HTMLInputElement = fixture.nativeElement.querySelector('#codigo');
    codigo.value = 'RECUPERACAO-1234';
    codigo.dispatchEvent(new Event('input'));
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);

    const request = http.expectOne('/api/v1/auth/login/recovery');
    expect(request.request.body).toEqual({ code: 'RECUPERACAO-1234' });
    request.flush({ detail: 'Código inválido.' }, { status: 401, statusText: 'Unauthorized' });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(codigo.value).toBe('');

    const volta = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((button) => button.textContent?.trim() === 'Voltar');
    volta?.click();
    fixture.detectChanges();
    expect((fixture.nativeElement.querySelector('#senha') as HTMLInputElement).value).toBe('');
  });
});
