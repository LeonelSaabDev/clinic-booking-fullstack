import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SpecialtyForm } from './specialty-form';

describe('SpecialtyForm', () => {
  let component: SpecialtyForm;
  let fixture: ComponentFixture<SpecialtyForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SpecialtyForm],
    }).compileComponents();

    fixture = TestBed.createComponent(SpecialtyForm);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
