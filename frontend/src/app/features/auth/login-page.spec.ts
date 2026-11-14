import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { LoginPage } from './login-page';

describe('LoginPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
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
});
