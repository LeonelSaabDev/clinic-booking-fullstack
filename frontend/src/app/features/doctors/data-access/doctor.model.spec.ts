import { Doctor, toDoctorRows } from './doctor.model';

describe('toDoctorRows', () => {
  const cardiology = { id: 'spec-1', name: 'Cardiology' };
  const osde = { id: 'hi-1', name: 'OSDE' };
  const galeno = { id: 'hi-2', name: 'Galeno' };

  const ana: Doctor = {
    id: 'doc-1',
    firstName: 'Ana',
    lastName: 'García',
    licenseNumber: 'MN 12345',
    specialtyId: 'spec-1',
    healthInsuranceIds: ['hi-1', 'hi-2'],
  };

  it('resolves specialty and health insurance ids to names', () => {
    const [row] = toDoctorRows([ana], [cardiology], [osde, galeno]);

    expect(row).toEqual({
      id: 'doc-1',
      fullName: 'García, Ana',
      licenseNumber: 'MN 12345',
      specialtyName: 'Cardiology',
      healthInsuranceNames: ['Galeno', 'OSDE'],
    });
  });

  it('returns an empty list of health insurances for a private-only doctor', () => {
    const [row] = toDoctorRows([{ ...ana, healthInsuranceIds: [] }], [cardiology], [osde]);

    expect(row.healthInsuranceNames).toEqual([]);
  });

  it('falls back gracefully when a referenced specialty is missing', () => {
    const [row] = toDoctorRows([ana], [], [osde, galeno]);

    expect(row.specialtyName).toBe('Unknown specialty');
  });
});
