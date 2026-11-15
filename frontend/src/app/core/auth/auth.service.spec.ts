import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { AuthService, CurrentUser } from './auth.service';

describe('AuthService', () => {
  let auth: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting()],
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('does not establish a session while two-factor verification is pending', async () => {
    const response = firstValueFrom(auth.login('staff@example.org', 'password'));

    const request = http.expectOne('/api/v1/auth/login');
    expect(request.request.body).toEqual({ email: 'staff@example.org', password: 'password' });
    request.flush({ requiresTwoFactor: true, user: null });

    expect(await response).toEqual({ requiresTwoFactor: true, user: null });
    expect(auth.isLoggedIn()).toBe(false);
  });

  it('refreshes the current user from the API on request', async () => {
    const currentUser: CurrentUser = {
      id: 'u1',
      email: 'staff@example.org',
      roles: ['Professional'],
      professionalId: 'p1',
      patientId: null,
      twoFactorEnabled: true,
      mustSetupTwoFactor: false,
    };

    const initialRequest = firstValueFrom(auth.loadSession());
    http.expectOne('/api/v1/auth/me').flush(currentUser);
    await initialRequest;

    const refreshRequest = firstValueFrom(auth.refreshSession());
    http.expectOne('/api/v1/auth/me').flush(currentUser);

    expect(await refreshRequest).toEqual(currentUser);
  });
});
