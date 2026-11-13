export type AppointmentStatus = 'Scheduled' | 'Confirmed' | 'Completed' | 'NoShow' | 'Cancelled' | 'Rescheduled';
export type ChargeReason = 'None' | 'LateCancellation' | 'NoShow';
export type CancellationSource = 'Patient' | 'Professional' | 'Platform';

export interface AgendaItem {
  id: string;
  patientId: string;
  patientDisplayName: string;
  startsAt: string;
  endsAt: string;
  status: AppointmentStatus;
  chargePercent: number;
  chargeReason: ChargeReason;
}

export interface ProfessionalSummary {
  id: string;
  professionalName: string;
  canAttend: boolean;
}