export type DayOfWeek =
  | "Monday"
  | "Tuesday"
  | "Wednesday"
  | "Thursday"
  | "Friday"
  | "Saturday"
  | "Sunday";

/** Clinic week order, which starts on Monday (the API sends day names, not numbers). */
export const DAYS_OF_WEEK: readonly DayOfWeek[] = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
];

/** Allowed by the domain: 10 to 120 minutes, in steps of 5. */
export const APPOINTMENT_DURATION_OPTIONS: readonly number[] = [10, 15, 20, 30, 40, 45, 60, 90, 120];

export interface ScheduleBlock {
  day: DayOfWeek;
  /** Clinic local time, "HH:mm" or "HH:mm:ss". */
  start: string;
  end: string;
}

export interface DoctorSchedule {
  appointmentDurationMinutes: number;
  blocks: ScheduleBlock[];
}

export interface AvailableSlot {
  /** ISO 8601 with the clinic's offset, e.g. "2026-10-21T09:00:00-03:00". */
  startsAt: string;
  endsAt: string;
}

/** "09:00:00" -> "09:00", the format an <input type="time"> expects. */
export function toInputTime(time: string): string {
  return time.slice(0, 5);
}
