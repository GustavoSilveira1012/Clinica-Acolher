export type ClinicalNoteStatus = 'Draft' | 'Finalized';

export interface ClinicalNoteSummary {
  id: string;
  appointmentId: string | null;
  status: ClinicalNoteStatus;
  createdAt: string;
  lastSavedAt: string;
  finalizedAt: string | null;
  revisionCount: number;
  addendumCount: number;
}

export interface ClinicalNoteAddendum {
  number: number;
  content: string;
  createdAt: string;
}

export interface ClinicalNote {
  id: string;
  patientId: string;
  appointmentId: string | null;
  status: ClinicalNoteStatus;
  content: string;
  revisionNumber: number;
  createdAt: string;
  lastSavedAt: string;
  finalizedAt: string | null;
  addenda: ClinicalNoteAddendum[];
}

export interface PatientBasics {
  id: string;
  fullName: string;
  socialName: string | null;
  active: boolean;
}

export interface RelationshipBasics {
  id: string;
  status: 'Active' | 'Suspended' | 'Ended';
}