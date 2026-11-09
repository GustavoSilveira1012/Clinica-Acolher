import {HttpClient} from '@angular/common/http';
import {Injectable, inject} from '@angular/core';
import {Observable} from 'rxjs';

export interface HealResponse {
    status: string;
    service: string;
    timestamp: string;
}

@Injectable({providedIn: 'root'})
export class HealthService {
    private readonly http = inject(HttpClient);

    check() : Observable<HealResponse> {
        return this.http.get<HealResponse>('/api/health');
    }
}