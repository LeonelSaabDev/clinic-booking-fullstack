import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { DoctorSchedulePage } from './doctor-schedule-page';

describe('DoctorSchedulePage', () => {
  const doctorUrl = `${environment.apiUrl}/doctors/doc-1`;
  let fixture: ComponentFixture<DoctorSchedulePage>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorSchedulePage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorSchedulePage);
    fixture.componentRef.setInput('id', 'doc-1');
    httpTesting = TestBed.inject(HttpTestingController);
    TestBed.tick();

    httpTesting.expectOne(doctorUrl).flush({
      id: 'doc-1',
      firstName: 'Ana',
      lastName: 'García',
      licenseNumber: 'MN 12345',
      specialtyId: 'spec-1',
      healthInsuranceIds: [],
    });
    httpTesting.expectOne(`${doctorUrl}/schedule`).flush({
      appointmentDurationMinutes: 45,
      blocks: [{ day: 'Wednesday', start: '09:00:00', end: '10:30:00' }],
    });
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('shows the doctor name and the free slots in clinic time', async () => {
    httpTesting
      .expectOne((request) => request.url === `${doctorUrl}/availability` && request.params.has('date'))
      .flush([
        { startsAt: '2026-10-21T09:00:00-03:00', endsAt: '2026-10-21T09:45:00-03:00' },
        { startsAt: '2026-10-21T09:45:00-03:00', endsAt: '2026-10-21T10:30:00-03:00' },
      ]);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('Ana García');
    expect(text).toContain('09:00');
    expect(text).toContain('09:45');
  });

  it('tells when there are no free slots on the chosen day', async () => {
    httpTesting.expectOne((request) => request.url === `${doctorUrl}/availability`).flush([]);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('No free slots on this day.');
  });
});
