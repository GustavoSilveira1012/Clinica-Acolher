import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ConvitePortal } from './convite-portal';

describe('ConvitePortal', () => {
  it('monta o link com o código no fragmento', async () => {
    TestBed.configureTestingModule({
      imports: [ConvitePortal],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(ConvitePortal);
    fixture.componentRef.setInput('pacienteId', 'pac1');
    fixture.detectChanges();

    fixture.nativeElement.querySelector('button').click();
    http.expectOne('/api/v1/patients/pac1/invitations').flush({ token: 'abc', expiresAt: '2026-11-24T12:00:00Z' });
    await fixture.whenStable();
    fixture.detectChanges();

    const campo: HTMLInputElement = fixture.nativeElement.querySelector('#link-convite');
    expect(campo.value).toBe(`${location.origin}/convite#abc`);
    http.verify();
  });
});