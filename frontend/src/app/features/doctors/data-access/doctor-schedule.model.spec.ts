import { toInputTime } from './doctor-schedule.model';

describe('doctor schedule helpers', () => {
  it('trims seconds so the value fits an input of type time', () => {
    expect(toInputTime('09:30:00')).toBe('09:30');
  });
});
