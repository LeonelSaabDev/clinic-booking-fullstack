import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { DoctorFormPage } from './doctor-form-page';

describe('DoctorFormPage', () => {
  let fixture: ComponentFixture<DoctorFormPage>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorFormPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorFormPage);
    httpTesting = TestBed.inject(HttpTestingController);
    TestBed.tick();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('asks to create a specialty first when there are none', async () => {
    httpTesting.expectOne(`${environment.apiUrl}/specialties`).flush([]);
    httpTesting.expectOne(`${environment.apiUrl}/health-insurances`).flush([]);
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Create a specialty first');
  });

  it('shows the form once specialties are available', async () => {
    httpTesting.expectOne(`${environment.apiUrl}/specialties`).flush([{ id: 'spec-1', name: 'Cardiology' }]);
    httpTesting.expectOne(`${environment.apiUrl}/health-insurances`).flush([]);
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('app-doctor-form')).not.toBeNull();
    expect(element.textContent).toContain('Cardiology');
  });
});
