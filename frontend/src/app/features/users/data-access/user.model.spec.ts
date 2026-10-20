import { Doctor } from '../../doctors/data-access/doctor.model';
import { doctorsWithoutAccount, toUserRows, User } from './user.model';

describe('user model', () => {
  const doctor = (id: string, lastName: string): Doctor => ({
    id,
    firstName: 'Ana',
    lastName,
    licenseNumber: 'MN',
    specialtyId: 's',
    healthInsuranceIds: [],
  });
  const garcia = doctor('doc-1', 'García');
  const rossi = doctor('doc-2', 'Rossi');
  const users: User[] = [
    { id: 'u-1', email: 'ana@clinica.com', role: 'Doctor', doctorId: 'doc-1' },
    { id: 'u-2', email: 'recepcion@clinica.com', role: 'Receptionist', doctorId: null },
  ];

  it('shows the doctor each account belongs to', () => {
    const rows = toUserRows(users, [garcia, rossi]);

    expect(rows.map((r) => r.doctorName)).toEqual(['García, Ana', null]);
  });

  it('only offers doctors that do not have an account yet', () => {
    expect(doctorsWithoutAccount([garcia, rossi], users)).toEqual([rossi]);
  });
});
