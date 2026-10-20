import { provideHttpClient } from "@angular/common/http";
import {
  HttpTestingController,
  provideHttpClientTesting,
} from "@angular/common/http/testing";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { provideRouter } from "@angular/router";
import { environment } from "../../../../../environments/environment";
import { HealthInsuranceListPage } from "./health-insurance-list-page";

describe("HealthInsuranceListPage", () => {
  let component: HealthInsuranceListPage;
  let fixture: ComponentFixture<HealthInsuranceListPage>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HealthInsuranceListPage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HealthInsuranceListPage);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);

    TestBed.tick();
    httpTesting.expectOne(`${environment.apiUrl}/health-insurances`).flush([]);
    await fixture.whenStable();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });

  it("should show the empty state when there are no health insurances", () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain("No health insurances yet.");
  });
});
