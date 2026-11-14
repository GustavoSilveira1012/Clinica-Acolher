import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, finalize, of, tap } from 'rxjs';

export interface CurrentUser {
  id: string;
  email: string;
  roles: string[];
  professionalId: string | null;
  patientId: string | null;
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

  login(email: string, password: string): Observable<CurrentUser> {
    return this.http
      .post<CurrentUser>('/api/v1/auth/login', { email, password })
      .pipe(tap((usuario) => this.definir(usuario)));
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