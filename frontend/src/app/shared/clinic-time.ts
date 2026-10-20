/**
 * Reads the time straight from an ISO instant like "2026-10-21T09:45:00-03:00" instead of converting it
 * with Date, so it always shows in clinic time, whatever the viewer's own time zone is.
 */
export function toClinicTime(instant: string): string {
  return instant.slice(11, 16);
}

/** The calendar day of an ISO instant, as written by the API in clinic time: "2026-10-21". */
export function toClinicDate(instant: string): string {
  return instant.slice(0, 10);
}

/** A local calendar date as "yyyy-MM-dd". Avoids toISOString(), which would shift to the UTC day. */
export function toIsoDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${date.getFullYear()}-${month}-${day}`;
}

export function todayAsIsoDate(today = new Date()): string {
  return toIsoDate(today);
}

export function tomorrowAsIsoDate(today = new Date()): string {
  return toIsoDate(new Date(today.getFullYear(), today.getMonth(), today.getDate() + 1));
}
