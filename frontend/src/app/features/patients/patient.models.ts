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