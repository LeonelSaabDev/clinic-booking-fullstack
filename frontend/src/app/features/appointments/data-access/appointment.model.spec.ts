import { Doctor } from '../../doctors/data-access/doctor.model';
import { Patient } from '../../patients/data-access/patient.model';
import { Appointment, coverageOptionFor, toAppointmentRows } from './appointment.model';

describe('appointment model', () => {
  const osde = { id: 'hi-1', name: 'OSDE' };
  const ana: Doctor = {
    id: 'doc-1',
    firstName: 'Ana',
    lastName: 'García',
    licenseNumber: 'MN 1',
    specialtyId: 'spec-1',
    healthInsuranceIds: ['hi-1'],
  };
  const juan: Patient = {
    id: 'pat-1',
    firstName: 'Juan',
    lastName: 'Pérez',
    documentNumber: '30123456',
    email: 'juan@mail.com',
    phone: '1155551234',
    healthInsuranceId: 'hi-1',
    memberNumber: '210-555',
  };
  const appointment: Appointment = {
    id: 'apt-1',
    doctorId: 'doc-1',
    patientId: 'pat-1',
    startsAt: '2026-10-21T09:00:00-03:00',
    endsAt: '2026-10-21T09:30:00-03:00',
    status: 'Scheduled',
    healthInsuranceId: 'hi-1',
    memberNumber: '210-555',
  };

  const twoDaysBefore = new Date('2026-10-19T09:00:00-03:00');

  it('builds a readable row in clinic time', () => {
    const [row] = toAppointmentRows([appointment], [ana], [juan], [osde], twoDaysBefore);

    expect(row).toEqual({
      id: 'apt-1',
      time: '09:00–09:30',
      doctorName: 'García, Ana',
      patientName: 'Pérez, Juan',
      documentNumber: '30123456',
      coverage: 'OSDE · 210-555',
      status: 'Scheduled',
      canCancel: true,
      canClose: false,
    });
  });

  it('stops offering cancellation inside the 24 hour notice window', () => {
    const [row] = toAppointmentRows([appointment], [ana], [juan], [osde], new Date('2026-10-20T10:00:00-03:00'));

    expect(row.canCancel).toBe(false);
    expect(row.canClose).toBe(false);
  });

  it('offers completing or no-show once the appointment started', () => {
    const [row] = toAppointmentRows([appointment], [ana], [juan], [osde], new Date('2026-10-21T09:05:00-03:00'));

    expect(row.canClose).toBe(true);
  });

  it('offers no action for an appointment that is already closed', () => {
    const [row] = toAppointmentRows(
      [{ ...appointment, status: 'Cancelled' }], [ana], [juan], [osde], twoDaysBefore);

    expect(row.canCancel).toBe(false);
    expect(row.canClose).toBe(false);
  });

  it('labels an appointment without coverage as private', () => {
    const [row] = toAppointmentRows([{ ...appointment, healthInsuranceId: null }], [ana], [juan], [osde]);

    expect(row.coverage).toBe('Private');
  });

  it('offers the patient coverage when the doctor accepts it', () => {
    expect(coverageOptionFor(ana, juan, [osde])).toEqual({
      kind: 'accepted',
      healthInsuranceName: 'OSDE',
      memberNumber: '210-555',
    });
  });

  it('explains when the doctor does not accept the patient coverage', () => {
    expect(coverageOptionFor({ ...ana, healthInsuranceIds: [] }, juan, [osde])).toEqual({
      kind: 'not-accepted',
      healthInsuranceName: 'OSDE',
    });
  });

  it('only offers private care to a patient without health insurance', () => {
    expect(coverageOptionFor(ana, { ...juan, healthInsuranceId: null }, [osde])).toEqual({ kind: 'private-only' });
  });
});
