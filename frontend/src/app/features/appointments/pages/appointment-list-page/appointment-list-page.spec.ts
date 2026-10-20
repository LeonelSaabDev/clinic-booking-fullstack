import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { AppointmentListPage } from './appointment-list-page';

describe('AppointmentListPage', () => {
  const api = environment.apiUrl;
  let fixture: ComponentFixture<AppointmentListPage>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppointmentListPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentListPage);
    fixture.componentRef.setInput('date', '2026-10-21');
    httpTesting = TestBed.inject(HttpTestingController);
    TestBed.tick();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('loads the day from the URL and shows its appointments by name', async () => {
    httpTesting
      .expectOne((request) => request.url === `${api}/appointments` && request.params.get('date') === '2026-10-21')
      .flush([
        {
          id: 'apt-1',
          doctorId: 'doc-1',
          patientId: 'pat-1',
          startsAt: '2026-10-21T09:00:00-03:00',
          endsAt: '2026-10-21T09:30:00-03:00',
          status: 'Scheduled',
          healthInsuranceId: null,
          memberNumber: null,
        },
      ]);
    httpTesting.expectOne(`${api}/doctors`).flush([
      { id: 'doc-1', firstName: 'Ana', lastName: 'García', licenseNumber: 'MN 1', specialtyId: 's', healthInsuranceIds: [] },
    ]);
    httpTesting.expectOne(`${api}/patients`).flush([
      {
        id: 'pat-1',
        firstName: 'Juan',
        lastName: 'Pérez',
        documentNumber: '30123456',
        email: 'juan@mail.com',
        phone: '1',
        healthInsuranceId: null,
        memberNumber: null,
      },
    ]);
    httpTesting.expectOne(`${api}/health-insurances`).flush([]);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('09:00–09:30');
    expect(text).toContain('Pérez, Juan');
    expect(text).toContain('García, Ana');
    expect(text).toContain('Private');
  });
});
