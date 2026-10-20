import { toClinicDate, toClinicTime, todayAsIsoDate, tomorrowAsIsoDate } from './clinic-time';

describe('clinic time helpers', () => {
  it('shows the clinic time written in the instant, ignoring the viewer time zone', () => {
    expect(toClinicTime('2026-10-21T09:45:00-03:00')).toBe('09:45');
  });

  it('reads the clinic day written in the instant', () => {
    expect(toClinicDate('2026-10-21T23:30:00-03:00')).toBe('2026-10-21');
  });

  it('formats today with the local calendar, even late at night', () => {
    expect(todayAsIsoDate(new Date(2026, 9, 21, 23, 30))).toBe('2026-10-21');
  });

  it('computes tomorrow, including across a month boundary', () => {
    expect(tomorrowAsIsoDate(new Date(2026, 9, 31, 23, 30))).toBe('2026-11-01');
  });
});
