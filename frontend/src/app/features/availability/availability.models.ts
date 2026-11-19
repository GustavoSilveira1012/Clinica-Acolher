import { Weekday } from '../agenda/week';

export interface AvailabilityWindow {
  weekday: Weekday;
  start: string; // "09:00:00"
  end: string;
}

export interface Availability {
  professionalId: string;
  timeZoneId: string;
  defaultSessionMinutes: number;
  breakMinutes: number;
  windows: AvailabilityWindow[];
  updatedAt: string;
}

export interface AvailabilityBlock {
  id: string;
  startsAt: string;
  endsAt: string;
  reason: string | null;
}

export interface BlockCreated {
  block: AvailabilityBlock;
  affectedAppointments: number;
}

export interface AvailabilityCheck {
  configured: boolean;
  withinAvailability: boolean;
  blocked: boolean;
}