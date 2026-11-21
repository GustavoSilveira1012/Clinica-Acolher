import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface InvitationIssued {
  token: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class PatientAccessApi {
  private readonly http = inject(HttpClient);

  issueInvitation(patientId: string): Observable<InvitationIssued> {
    return this.http.post<InvitationIssued>(`/api/v1/patients/${patientId}/invitations`, {});
  }

  accept(token: string, password: string): Observable<unknown> {
    return this.http.post('/api/v1/patient-invitations/accept', { token, password });
  }
}