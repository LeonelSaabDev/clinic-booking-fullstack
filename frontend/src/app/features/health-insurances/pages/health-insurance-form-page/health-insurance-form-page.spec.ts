import { ComponentFixture, TestBed } from "@angular/core/testing";
import { provideRouter } from "@angular/router";
import { HealthInsuranceFormPage } from "./health-insurance-form-page";

describe("HealthInsuranceFormPage", () => {
  let component: HealthInsuranceFormPage;
  let fixture: ComponentFixture<HealthInsuranceFormPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HealthInsuranceFormPage],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(HealthInsuranceFormPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
