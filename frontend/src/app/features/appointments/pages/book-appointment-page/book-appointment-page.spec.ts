import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { BookAppointmentPage } from './book-appointment-page';

describe('BookAppointmentPage', () => {
  const api = environment.apiUrl;
  const nine = { startsAt: '2026-10-21T09:00:00-03:00', endsAt: '2026-10-21T09:30:00-03:00' };
  const juan = {
    id: 'pat-1',
    firstName: 'Juan',
    lastName: 'Pérez',
    documentNumber: '30123456',
    email: 'juan@mail.com',
    phone: '1155551234',
    healthInsuranceId: 'hi-1',
    memberNumber: '210-555',
  };

  let fixture: ComponentFixture<BookAppointmentPage>;
  let element: HTMLElement;
  let httpTesting: HttpTestingController;
  let navigate: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BookAppointmentPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(BookAppointmentPage);
    element = fixture.nativeElement as HTMLElement;
    httpTesting = TestBed.inject(HttpTestingController);
    TestBed.tick();

    httpTesting.expectOne(`${api}/doctors`).flush([
      { id: 'doc-1', firstName: 'Ana', lastName: 'García', licenseNumber: 'MN 1', specialtyId: 's', healthInsuranceIds: ['hi-1'] },
    ]);
    httpTesting.expectOne(`${api}/health-insurances`).flush([{ id: 'hi-1', name: 'OSDE' }]);
    await fixture.whenStable();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  function setValue(selector: string, value: string): void {
    const control = element.querySelector<HTMLInputElement | HTMLSelectElement>(selector)!;
    control.value = value;
    control.dispatchEvent(new Event('change'));
  }

  async function chooseDoctorSlotAndPatient(): Promise<void> {
    setValue('#date', '2026-10-21');
    setValue('#doctor', 'doc-1');
    TestBed.tick();
    httpTesting.expectOne(`${api}/doctors/doc-1/availability?date=2026-10-21`).flush([nine]);
    await fixture.whenStable();
    element.querySelector<HTMLButtonElement>('.slot')!.click();

    element.querySelector<HTMLInputElement>('input[type="search"]')!.value = 'Pérez';
    element.querySelector<HTMLButtonElement>('form.search button')!.click();
    TestBed.tick();
    httpTesting.expectOne((request) => request.url === `${api}/patients` && request.params.get('search') === 'Pérez').flush([juan]);
    await fixture.whenStable();
    element.querySelector<HTMLButtonElement>('.result')!.click();
    await fixture.whenStable();
  }

  function bookButton(): HTMLButtonElement {
    return element.querySelector<HTMLButtonElement>('.form-actions button')!;
  }

  it('keeps the book button disabled until doctor, slot and patient are chosen', () => {
    expect(bookButton().disabled).toBe(true);
  });

  it('books with the accepted coverage and returns to that day', async () => {
    await chooseDoctorSlotAndPatient();
    expect(element.textContent).toContain('Use OSDE · member 210-555');

    bookButton().click();
    const request = httpTesting.expectOne(`${api}/appointments`);
    expect(request.request.body).toEqual({
      doctorId: 'doc-1',
      patientId: 'pat-1',
      startsAt: nine.startsAt,
      useHealthInsurance: true,
    });
    request.flush({ id: 'apt-1' });

    expect(navigate).toHaveBeenCalledWith(['/appointments'], { queryParams: { date: '2026-10-21' } });
  });

  it('refreshes the free slots when someone else took the slot first', async () => {
    await chooseDoctorSlotAndPatient();

    bookButton().click();
    httpTesting
      .expectOne(`${api}/appointments`)
      .flush({ detail: 'That slot was just booked by someone else.' }, { status: 409, statusText: 'Conflict' });
    TestBed.tick();
    httpTesting.expectOne(`${api}/doctors/doc-1/availability?date=2026-10-21`).flush([]);
    await fixture.whenStable();

    expect(element.textContent).toContain('That slot was just booked by someone else.');
    expect(element.textContent).toContain('No free slots on this day.');
    expect(navigate).not.toHaveBeenCalled();
  });
});
