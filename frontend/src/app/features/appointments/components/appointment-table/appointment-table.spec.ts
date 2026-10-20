import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppointmentTable } from './appointment-table';

describe('AppointmentTable', () => {
  let fixture: ComponentFixture<AppointmentTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentTable],
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentTable);
  });

  it('tells when the day has no appointments', async () => {
    fixture.componentRef.setInput('appointments', []);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('No appointments on this day.');
  });

  it('shows time, patient, doctor, coverage and status', async () => {
    fixture.componentRef.setInput('appointments', [
      {
        id: 'apt-1',
        time: '09:00–09:30',
        doctorName: 'García, Ana',
        patientName: 'Pérez, Juan',
        documentNumber: '30123456',
        coverage: 'OSDE · 210-555',
        status: 'Scheduled',
      },
    ]);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('09:00–09:30');
    expect(text).toContain('Pérez, Juan');
    expect(text).toContain('García, Ana');
    expect(text).toContain('OSDE · 210-555');
    expect(text).toContain('Scheduled');
  });
});
