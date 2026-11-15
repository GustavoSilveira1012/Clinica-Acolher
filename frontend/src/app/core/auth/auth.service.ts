import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, map, of, tap } from 'rxjs';

export interface CurrentUser {
  id: string;
  email: string;
  roles: string[];
  professionalId: string | null;
  patientId: string | null;
  twoFactorEnabled: boolean;
  mustSetupTwoFactor: boolean;
}

export interface LoginResponse {
  requiresTwoFactor: boolean;
  user: CurrentUser | null;
}

export interface TwoFactorSetup {
  manualKey: string;
  qrCodeDataUri: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly _user = signal<CurrentUser | null>(null);
  private sessaoConsultada = false;

  readonly user = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._user() !== null);

  /** Descobre se o navegador já tem uma sessão válida. Consulta a API só na primeira vez. */
  loadSession(): Observable<CurrentUser | null> {
    if (this.sessaoConsultada) return of(this._user());

    return this.http.get<CurrentUser>('/api/v1/auth/me').pipe(
      catchError(() => of(null)),
      tap((usuario) => this.definir(usuario)),
    );
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>('/api/v1/auth/login', { email, password })
      .pipe(tap((resposta) => resposta.user && this.definir(resposta.user)));
  }

  loginWithCode(code: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>('/api/v1/auth/login/2fa', { code })
      .pipe(tap((resposta) => resposta.user && this.definir(resposta.user)));
  }

  loginWithRecoveryCode(code: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>('/api/v1/auth/login/recovery', { code })
      .pipe(tap((resposta) => resposta.user && this.definir(resposta.user)));
  }

  setupTwoFactor(): Observable<TwoFactorSetup> {
    return this.http.post<TwoFactorSetup>('/api/v1/auth/2fa/setup', {});
  }

  enableTwoFactor(code: string): Observable<string[]> {
    return this.http
      .post<{ recoveryCodes: string[] }>('/api/v1/auth/2fa/enable', { code })
      .pipe(map((resposta) => resposta.recoveryCodes));
  }

  /** Consulta o /me de novo, por exemplo depois de ativar o 2FA. */
  refreshSession(): Observable<CurrentUser | null> {
    this.sessaoConsultada = false;
    return this.loadSession();
  }

  logout(): Observable<unknown> {
    return this.http.post('/api/v1/auth/logout', {}).pipe(finalize(() => this.clearSession()));
  }

  clearSession(): void {
    this.definir(null);
  }

  private definir(usuario: CurrentUser | null): void {
    this._user.set(usuario);
    this.sessaoConsultada = true;
  }
}