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
        canCancel: false,
        canClose: false,
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

  it('offers only the actions allowed for each appointment', async () => {
    const base = {
      doctorName: 'García, Ana',
      patientName: 'Pérez, Juan',
      documentNumber: '30123456',
      coverage: 'Private',
      status: 'Scheduled' as const,
    };
    fixture.componentRef.setInput('appointments', [
      { ...base, id: 'future', time: '09:00–09:30', canCancel: true, canClose: false },
      { ...base, id: 'started', time: '10:00–10:30', canCancel: false, canClose: true },
    ]);
    await fixture.whenStable();

    const rows = (fixture.nativeElement as HTMLElement).querySelectorAll('tbody tr');
    expect(rows[0].textContent).toContain('Cancel');
    expect(rows[0].textContent).not.toContain('Complete');
    expect(rows[1].textContent).toContain('Complete');
    expect(rows[1].textContent).toContain('No-show');
    expect(rows[1].textContent).not.toContain('Cancel');
  });

  it('emits the appointment to cancel', async () => {
    const row = {
      id: 'apt-1',
      time: '09:00–09:30',
      doctorName: 'García, Ana',
      patientName: 'Pérez, Juan',
      documentNumber: '30123456',
      coverage: 'Private',
      status: 'Scheduled' as const,
      canCancel: true,
      canClose: false,
    };
    fixture.componentRef.setInput('appointments', [row]);
    await fixture.whenStable();
    let cancelled: unknown;
    fixture.componentInstance.cancel.subscribe((appointment) => (cancelled = appointment));

    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('.actions button')!.click();

    expect(cancelled).toEqual(row);
  });
});
