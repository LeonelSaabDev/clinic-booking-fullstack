import { ComponentFixture, TestBed } from "@angular/core/testing";
import { provideRouter } from "@angular/router";
import { SpecialtyFormPage } from "./specialty-form-page";

describe("SpecialtyFormPage", () => {
  let component: SpecialtyFormPage;
  let fixture: ComponentFixture<SpecialtyFormPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SpecialtyFormPage],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(SpecialtyFormPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
