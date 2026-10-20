import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { PatientListPage } from './patient-list-page';

describe('PatientListPage', () => {
  const patientsUrl = `${environment.apiUrl}/patients`;
  let fixture: ComponentFixture<PatientListPage>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientListPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientListPage);
    httpTesting = TestBed.inject(HttpTestingController);
    TestBed.tick();
    httpTesting.expectOne(`${environment.apiUrl}/health-insurances`).flush([{ id: 'hi-1', name: 'OSDE' }]);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('lists patients with their coverage', async () => {
    httpTesting.expectOne(patientsUrl).flush([
      {
        id: 'pat-1',
        firstName: 'Juan',
        lastName: 'Pérez',
        documentNumber: '30123456',
        email: 'juan@mail.com',
        phone: '11 5555-1234',
        healthInsuranceId: 'hi-1',
        memberNumber: '210-555',
      },
    ]);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('OSDE · 210-555');
  });

  it('searches the API with the typed term', async () => {
    httpTesting.expectOne(patientsUrl).flush([]);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    element.querySelector<HTMLInputElement>('input[type="search"]')!.value = '  3012 ';
    element.querySelector<HTMLButtonElement>('form.search button')!.click();
    TestBed.tick();

    httpTesting
      .expectOne((request) => request.url === patientsUrl && request.params.get('search') === '3012')
      .flush([]);
    await fixture.whenStable();
    expect(element.textContent).toContain('No patients found.');
  });
});
