import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Availability, AvailabilityBlock, AvailabilityCheck, AvailabilityWindow, BlockCreated } from './availability.models';

@Injectable({ providedIn: 'root' })
export class AvailabilityApi {
  private readonly http = inject(HttpClient);

  get(professionalId: string): Observable<Availability> {
    return this.http.get<Availability>(`/api/v1/professionals/${professionalId}/availability`);
  }

  save(
    professionalId: string,
    body: { timeZoneId: string; defaultSessionMinutes: number; breakMinutes: number; windows: AvailabilityWindow[] },
  ): Observable<Availability> {
    return this.http.put<Availability>(`/api/v1/professionals/${professionalId}/availability`, body);
  }

  check(professionalId: string, startsAt: Date, durationMinutes: number): Observable<AvailabilityCheck> {
    const params = new HttpParams().set('startsAt', startsAt.toISOString()).set('durationMinutes', durationMinutes);
    return this.http.get<AvailabilityCheck>(`/api/v1/professionals/${professionalId}/availability/check`, { params });
  }

  listBlocks(professionalId: string, from: Date, to: Date): Observable<AvailabilityBlock[]> {
    const params = new HttpParams().set('from', from.toISOString()).set('to', to.toISOString());
    return this.http.get<AvailabilityBlock[]>(`/api/v1/professionals/${professionalId}/availability-blocks`, { params });
  }

  createBlock(professionalId: string, startsAt: Date, endsAt: Date, reason: string | null): Observable<BlockCreated> {
    return this.http.post<BlockCreated>(`/api/v1/professionals/${professionalId}/availability-blocks`, {
      startsAt: startsAt.toISOString(),
      endsAt: endsAt.toISOString(),
      reason,
    });
  }

  deleteBlock(blockId: string): Observable<unknown> {
    return this.http.delete(`/api/v1/availability-blocks/${blockId}`);
  }
}