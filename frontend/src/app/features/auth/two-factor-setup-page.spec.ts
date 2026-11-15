import { TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TwoFactorSetupPage } from './two-factor-setup-page';

describe('TwoFactorSetupPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TwoFactorSetupPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('ativa o 2FA, mostra os códigos e só conclui depois de confirmar que foram guardados', async () => {
    const fixture = TestBed.createComponent(TwoFactorSetupPage);
    fixture.detectChanges();

    http.expectOne('/api/v1/auth/2fa/setup').flush({
      manualKey: 'abcd efgh ijkl mnop',
      qrCodeDataUri: 'data:image/png;base64,AAAA',
    });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('img').getAttribute('src')).toBe('data:image/png;base64,AAAA');

    const campo: HTMLInputElement = fixture.nativeElement.querySelector('#codigo');
    campo.value = '123456';
    campo.dispatchEvent(new Event('input'));
    fixture.debugElement.query(By.css('form')).triggerEventHandler('ngSubmit', null);

    const codigos = Array.from({ length: 10 }, (_, i) => `codigo-${i}`);
    http.expectOne('/api/v1/auth/2fa/enable').flush({ recoveryCodes: codigos });
    await fixture.whenStable();
    fixture.detectChanges();

    const texto: string = fixture.nativeElement.textContent;
    expect(texto).toContain('codigo-9');
    expect(fixture.nativeElement.querySelector('img')).toBeNull();

    const concluir = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((button) => button.textContent?.trim() === 'Concluir');
    expect(concluir).toBeDefined();
    expect(concluir!.disabled).toBe(true);
  });
});
