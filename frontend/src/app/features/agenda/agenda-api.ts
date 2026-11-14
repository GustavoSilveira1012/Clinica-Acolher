import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ActiveRelationship, AgendaItem, CancellationSource, ProfessionalSummary } from './agenda.models';
import { toSeriesSchedule } from './week';

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

  listActiveRelationships(professionalId: string): Observable<ActiveRelationship[]> {
    return this.http.get<ActiveRelationship[]>(`/api/v1/professionals/${professionalId}/relationships/active`);
  }

  schedule(relationshipId: string, startsAt: Date, durationMinutes: number): Observable<unknown> {
    return this.http.post('/api/v1/appointments', {
      relationshipId,
      startsAt: startsAt.toISOString(),
      durationMinutes,
    });
  }

  createSeries(relationshipId: string, startsAt: Date, durationMinutes: number, timeZoneId: string): Observable<unknown> {
    return this.http.post('/api/v1/series', {
      relationshipId,
      ...toSeriesSchedule(startsAt),
      durationMinutes,
      timeZoneId,
    });
  }

  endSeries(seriesId: string, lastDay: string): Observable<unknown> {
    return this.http.post(`/api/v1/series/${seriesId}/end`, { lastDay });
  }

  reschedule(id: string, startsAt: Date, durationMinutes: number): Observable<unknown> {
    return this.http.post(`/api/v1/appointments/${id}/reschedule`, {
      startsAt: startsAt.toISOString(),
      durationMinutes,
    });
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