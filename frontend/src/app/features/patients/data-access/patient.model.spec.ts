import { Patient, toPatientRows } from './patient.model';

describe('toPatientRows', () => {
  const juan: Patient = {
    id: 'pat-1',
    firstName: 'Juan',
    lastName: 'Pérez',
    documentNumber: '30123456',
    email: 'juan@mail.com',
    phone: '11 5555-1234',
    healthInsuranceId: 'hi-1',
    memberNumber: '210-555',
  };

  it('describes the coverage with the health insurance name and member number', () => {
    const [row] = toPatientRows([juan], [{ id: 'hi-1', name: 'OSDE' }]);

    expect(row.fullName).toBe('Pérez, Juan');
    expect(row.coverage).toBe('OSDE · 210-555');
  });

  it('leaves the coverage empty for a private patient', () => {
    const [row] = toPatientRows([{ ...juan, healthInsuranceId: null, memberNumber: null }], []);

    expect(row.coverage).toBeNull();
  });
});
