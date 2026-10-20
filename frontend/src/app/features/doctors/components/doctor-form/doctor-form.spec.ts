import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DoctorRequest } from '../../data-access/doctor.model';
import { DoctorForm } from './doctor-form';

describe('DoctorForm', () => {
  let fixture: ComponentFixture<DoctorForm>;
  let element: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorForm],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorForm);
    fixture.componentRef.setInput('specialties', [{ id: 'spec-1', name: 'Cardiology' }]);
    fixture.componentRef.setInput('healthInsurances', [
      { id: 'hi-1', name: 'OSDE' },
      { id: 'hi-2', name: 'Galeno' },
    ]);
    await fixture.whenStable();
    element = fixture.nativeElement as HTMLElement;
  });

  function type(selector: string, value: string): void {
    const input = element.querySelector<HTMLInputElement | HTMLSelectElement>(selector)!;
    input.value = value;
    input.dispatchEvent(new Event(input.tagName === 'SELECT' ? 'change' : 'input'));
  }

  it('does not emit save while required fields are empty', async () => {
    let saved: DoctorRequest | undefined;
    fixture.componentInstance.save.subscribe((request) => (saved = request));

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
    await fixture.whenStable();

    expect(saved).toBeUndefined();
    expect(element.textContent).toContain('First name is required.');
  });

  it('emits the request with the checked health insurances', async () => {
    let saved: DoctorRequest | undefined;
    fixture.componentInstance.save.subscribe((request) => (saved = request));

    type('#firstName', 'Ana');
    type('#lastName', 'García');
    type('#licenseNumber', 'MN 12345');
    type('#specialtyId', 'spec-1');
    const [osdeCheckbox] = element.querySelectorAll<HTMLInputElement>('input[type="checkbox"]');
    osdeCheckbox.click();
    await fixture.whenStable();

    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();

    expect(saved).toEqual({
      firstName: 'Ana',
      lastName: 'García',
      licenseNumber: 'MN 12345',
      specialtyId: 'spec-1',
      healthInsuranceIds: ['hi-1'],
    });
  });
});
