import { AppointmentStatus } from '../agenda/agenda.models';

export interface PortalAppointment {
  id: string;
  startsAt: string;
  endsAt: string;
  status: AppointmentStatus;
  professionalName: string;
}

export interface PortalAgenda {
  upcoming: PortalAppointment[];
  recent: PortalAppointment[];
}

export interface PortalPolicy {
  policyId: string;
  professionalName: string;
  version: number;
  minimumNoticeHours: number;
  lateCancellationChargePercent: number;
  noShowChargePercent: number;
  publishedAt: string;
  accepted: boolean;
  acceptedAt: string | null;
}