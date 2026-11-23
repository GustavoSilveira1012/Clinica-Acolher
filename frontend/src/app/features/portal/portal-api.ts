import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PortalAgenda, PortalPolicy } from './portal.models';

@Injectable({ providedIn: 'root' })
export class PortalApi {
  private readonly http = inject(HttpClient);

  getAgenda(): Observable<PortalAgenda> {
    return this.http.get<PortalAgenda>('/api/v1/portal/appointments');
  }

  listPolicies(): Observable<PortalPolicy[]> {
    return this.http.get<PortalPolicy[]>('/api/v1/portal/attendance-policies');
  }

  acceptPolicy(policyId: string): Observable<PortalPolicy> {
    return this.http.post<PortalPolicy>(`/api/v1/portal/attendance-policies/${policyId}/accept`, {});
  }
}