import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { PatientFormPage } from './patient-form-page';

describe('PatientFormPage', () => {
  let fixture: ComponentFixture<PatientFormPage>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientFormPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientFormPage);
    httpTesting = TestBed.inject(HttpTestingController);
    TestBed.tick();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('offers the health insurances in the registration form', async () => {
    httpTesting.expectOne(`${environment.apiUrl}/health-insurances`).flush([{ id: 'hi-1', name: 'OSDE' }]);
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('app-patient-form')).not.toBeNull();
    expect(element.textContent).toContain('Private (no health insurance)');
    expect(element.textContent).toContain('OSDE');
  });
});
