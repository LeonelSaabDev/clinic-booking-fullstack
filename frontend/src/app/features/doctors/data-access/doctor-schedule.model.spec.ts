import { toClinicTime, toInputTime, tomorrowAsIsoDate } from './doctor-schedule.model';

describe('doctor schedule helpers', () => {
  it('trims seconds so the value fits an input of type time', () => {
    expect(toInputTime('09:30:00')).toBe('09:30');
  });

  it('shows the clinic time written in the instant, ignoring the viewer time zone', () => {
    expect(toClinicTime('2026-10-21T09:45:00-03:00')).toBe('09:45');
  });

  it('computes tomorrow, including across a month boundary', () => {
    expect(tomorrowAsIsoDate(new Date(2026, 9, 31, 23, 30))).toBe('2026-11-01');
  });
});
