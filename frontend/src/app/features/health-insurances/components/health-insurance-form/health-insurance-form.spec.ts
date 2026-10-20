import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HealthInsuranceForm } from './health-insurance-form';

describe('HealthInsuranceForm', () => {
  let component: HealthInsuranceForm;
  let fixture: ComponentFixture<HealthInsuranceForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HealthInsuranceForm],
    }).compileComponents();

    fixture = TestBed.createComponent(HealthInsuranceForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
