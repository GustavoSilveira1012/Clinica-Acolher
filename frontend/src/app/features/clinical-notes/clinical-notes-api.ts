import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ClinicalNote, ClinicalNoteSummary, PatientBasics, RelationshipBasics } from './clinical-notes.models';

@Injectable({ providedIn: 'root' })
export class ClinicalNotesApi {
  private readonly http = inject(HttpClient);

  getPatient(id: string): Observable<PatientBasics> {
    return this.http.get<PatientBasics>(`/api/v1/patients/${id}`);
  }

  listRelationships(patientId: string): Observable<RelationshipBasics[]> {
    return this.http.get<RelationshipBasics[]>(`/api/v1/patients/${patientId}/relationships`);
  }

  listByPatient(patientId: string): Observable<ClinicalNoteSummary[]> {
    return this.http.get<ClinicalNoteSummary[]>(`/api/v1/patients/${patientId}/clinical-notes`);
  }

  get(id: string): Observable<ClinicalNote> {
    return this.http.get<ClinicalNote>(`/api/v1/clinical-notes/${id}`);
  }

  create(relationshipId: string, content: string): Observable<ClinicalNote> {
    return this.http.post<ClinicalNote>('/api/v1/clinical-notes', { relationshipId, appointmentId: null, content });
  }

  saveDraft(id: string, content: string): Observable<ClinicalNote> {
    return this.http.post<ClinicalNote>(`/api/v1/clinical-notes/${id}/revisions`, { content });
  }

  finalize(id: string): Observable<ClinicalNote> {
    return this.http.post<ClinicalNote>(`/api/v1/clinical-notes/${id}/finalize`, {});
  }

  addAddendum(id: string, content: string): Observable<ClinicalNote> {
    return this.http.post<ClinicalNote>(`/api/v1/clinical-notes/${id}/addenda`, { content });
  }
}