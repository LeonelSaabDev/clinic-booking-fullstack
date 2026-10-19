import { ComponentFixture, TestBed } from "@angular/core/testing";
import { SpecialtyTable } from "./specialty-table";

describe("SpecialtyTable", () => {
  let component: SpecialtyTable;
  let fixture: ComponentFixture<SpecialtyTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SpecialtyTable],
    }).compileComponents();

    fixture = TestBed.createComponent(SpecialtyTable);
    component = fixture.componentInstance;
    fixture.componentRef.setInput("specialties", []);
    await fixture.whenStable();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
