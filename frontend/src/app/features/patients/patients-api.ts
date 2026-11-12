import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult, PatientListItem, PatientRequest, PatientResponse, PatientSearchRequest } from './patient.models';

@Injectable({providedIn: 'root'})
export class PatientsApi {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/v1/patients';

    search(request: PatientSearchRequest): Observable<PagedResult<PatientListItem>> {
        return this.http.post<PagedResult<PatientListItem>>(`${this.baseUrl}/search`, request);
    }


    getById(id: string): Observable<PatientResponse> {
    return this.http.get<PatientResponse>(`${this.baseUrl}/${id}`);
    }

    create(request: PatientRequest): Observable<PatientResponse> {
    return this.http.post<PatientResponse>(this.baseUrl, request);
    }

    update(id: string, request: PatientRequest): Observable<PatientResponse> {
        return this.http.put<PatientResponse>(`${this.baseUrl}/${id}`, request);
    }

    deactivate(id: string, reason: string): Observable<PatientResponse> {
        return this.http.post<PatientResponse>(`${this.baseUrl}/${id}/deactivate`, { reason });
    }

    reactivate(id: string): Observable<PatientResponse> {
        return this.http.post<PatientResponse>(`${this.baseUrl}/${id}/reactivate`, {});
    }
}