import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { DoctorListPage } from './doctor-list-page';

describe('DoctorListPage', () => {
  let fixture: ComponentFixture<DoctorListPage>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorListPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorListPage);
    httpTesting = TestBed.inject(HttpTestingController);
    TestBed.tick();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('combines doctors, specialties and health insurances into readable rows', async () => {
    httpTesting.expectOne(`${environment.apiUrl}/doctors`).flush([
      {
        id: 'doc-1',
        firstName: 'Ana',
        lastName: 'García',
        licenseNumber: 'MN 12345',
        specialtyId: 'spec-1',
        healthInsuranceIds: ['hi-1'],
      },
    ]);
    httpTesting.expectOne(`${environment.apiUrl}/specialties`).flush([{ id: 'spec-1', name: 'Cardiology' }]);
    httpTesting.expectOne(`${environment.apiUrl}/health-insurances`).flush([{ id: 'hi-1', name: 'OSDE' }]);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('García, Ana');
    expect(text).toContain('Cardiology');
    expect(text).toContain('OSDE');
  });
});
