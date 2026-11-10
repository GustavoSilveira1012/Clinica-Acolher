import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult, PatientListItem, PatientSearchRequest } from './patient.models';

@Injectable({providedIn: 'root'})
export class PatientsApi {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = '/api/v1/patients';

    search(request: PatientSearchRequest): Observable<PagedResult<PatientListItem>> {
        return this.http.post<PagedResult<PatientListItem>>(`${this.baseUrl}/search`, request);
    }
}
