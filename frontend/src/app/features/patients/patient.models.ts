export interface PatientListItem {
    id: string;
    displayName: string;
    email: string | null;
    phone: string | null;
    active: boolean;
}

export interface PagedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
}

export interface PatientSearchRequest {
    search: string | null;
    active: boolean | null;
    page: number;
    pageSize: number;
}

export interface PatientResponse {
    id: string;
    displayName: string;
    fullName: string;
    socialName: string | null;
    email: string | null;
    phone: string | null;
    birthDate: string;
    active: boolean;
    createdAt: string;
}

export interface PatientRequest {
    fullName: string;
    birthDate: string;
    socialName: string | null;
    email: string | null;
    phone: string | null;
}
