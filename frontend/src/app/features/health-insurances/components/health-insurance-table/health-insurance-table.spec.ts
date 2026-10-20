import { ComponentFixture, TestBed } from "@angular/core/testing";
import { HealthInsuranceTable } from "./health-insurance-table";

describe("HealthInsuranceTable", () => {
  let component: HealthInsuranceTable;
  let fixture: ComponentFixture<HealthInsuranceTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HealthInsuranceTable],
    }).compileComponents();

    fixture = TestBed.createComponent(HealthInsuranceTable);
    component = fixture.componentInstance;
    fixture.componentRef.setInput("healthInsurances", []);
    await fixture.whenStable();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
