import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PatientRow } from '../../data-access/patient.model';
import { PatientTable } from './patient-table';

describe('PatientTable', () => {
  let fixture: ComponentFixture<PatientTable>;

  const juan: PatientRow = {
    id: 'pat-1',
    fullName: 'Pérez, Juan',
    documentNumber: '30123456',
    email: 'juan@mail.com',
    phone: '11 5555-1234',
    coverage: 'OSDE · 210-555',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientTable],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientTable);
  });

  async function render(patients: PatientRow[]): Promise<string> {
    fixture.componentRef.setInput('patients', patients);
    await fixture.whenStable();
    return (fixture.nativeElement as HTMLElement).textContent ?? '';
  }

  it('shows the empty state when no patient matches', async () => {
    expect(await render([])).toContain('No patients found.');
  });

  it('shows identity, contact and coverage', async () => {
    const text = await render([juan]);

    expect(text).toContain('Pérez, Juan');
    expect(text).toContain('DNI 30123456');
    expect(text).toContain('juan@mail.com');
    expect(text).toContain('OSDE · 210-555');
  });

  it('labels a patient without coverage as private', async () => {
    expect(await render([{ ...juan, coverage: null }])).toContain('Private');
  });
});
