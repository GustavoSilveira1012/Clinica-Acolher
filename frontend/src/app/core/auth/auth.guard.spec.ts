import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { Observable, firstValueFrom } from 'rxjs';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  function executarGuard(url: string): Promise<boolean | UrlTree> {
    const resultado = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    ) as Observable<boolean | UrlTree>;
    return firstValueFrom(resultado);
  }

  it('manda para o login quem não tem sessão, lembrando para onde ia', async () => {
    const promessa = executarGuard('/pacientes');

    http.expectOne('/api/v1/auth/me').flush(null, { status: 401, statusText: 'Unauthorized' });
    const resultado = (await promessa) as UrlTree;

    expect(resultado.root.children['primary'].segments[0].path).toBe('login');
    expect(resultado.queryParams['voltar']).toBe('/pacientes');
  });

  it('libera quem tem sessão', async () => {
    const promessa = executarGuard('/agenda');

    http.expectOne('/api/v1/auth/me').flush({
      id: 'u1', email: 'profissional@exemplo.org', roles: [], professionalId: 'p1', patientId: null,
    });

    expect(await promessa).toBe(true);
  });
});
