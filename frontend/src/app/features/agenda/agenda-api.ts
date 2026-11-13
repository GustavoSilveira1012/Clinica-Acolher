import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AgendaItem, CancellationSource, ProfessionalSummary } from './agenda.models';

@Injectable({ providedIn: 'root' })
export class AgendaApi {
  private readonly http = inject(HttpClient);

  listProfessionals(): Observable<ProfessionalSummary[]> {
    return this.http.get<ProfessionalSummary[]>('/api/v1/professionals');
  }

  getAgenda(professionalId: string, from: Date, to: Date): Observable<AgendaItem[]> {
    const params = new HttpParams().set('from', from.toISOString()).set('to', to.toISOString());
    return this.http.get<AgendaItem[]>(`/api/v1/professionals/${professionalId}/agenda`, { params });
  }

  confirm(id: string): Observable<unknown> {
    return this.http.post(`/api/v1/appointments/${id}/confirm`, {});
  }

  complete(id: string): Observable<unknown> {
    return this.http.post(`/api/v1/appointments/${id}/complete`, {});
  }

  noShow(id: string): Observable<unknown> {
    return this.http.post(`/api/v1/appointments/${id}/no-show`, {});
  }

  cancel(id: string, by: CancellationSource, reason: string): Observable<unknown> {
    return this.http.post(`/api/v1/appointments/${id}/cancel`, { by, reason });
  }
}