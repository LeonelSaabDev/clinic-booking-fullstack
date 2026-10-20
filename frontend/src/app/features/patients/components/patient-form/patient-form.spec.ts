import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PatientRequest } from '../../data-access/patient.model';
import { PatientForm } from './patient-form';

describe('PatientForm', () => {
  let fixture: ComponentFixture<PatientForm>;
  let element: HTMLElement;
  let saved: PatientRequest | undefined;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientForm],
    }).compileComponents();

    fixture = TestBed.createComponent(PatientForm);
    fixture.componentRef.setInput('healthInsurances', [{ id: 'hi-1', name: 'OSDE' }]);
    fixture.componentInstance.save.subscribe((request) => (saved = request));
    saved = undefined;
    await fixture.whenStable();
    element = fixture.nativeElement as HTMLElement;
  });

  function setValue(selector: string, value: string): void {
    const control = element.querySelector<HTMLInputElement | HTMLSelectElement>(selector)!;
    control.value = value;
    control.dispatchEvent(new Event(control.tagName === 'SELECT' ? 'change' : 'input'));
  }

  function fillIdentity(): void {
    setValue('#firstName', 'Juan');
    setValue('#lastName', 'Pérez');
    setValue('#documentNumber', '30.123.456');
    setValue('#email', 'juan@mail.com');
    setValue('#phone', '11 5555-1234');
  }

  function submit(): void {
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
  }

  it('emits a private patient with null coverage', async () => {
    fillIdentity();
    submit();

    expect(saved).toEqual({
      firstName: 'Juan',
      lastName: 'Pérez',
      documentNumber: '30.123.456',
      email: 'juan@mail.com',
      phone: '11 5555-1234',
      healthInsuranceId: null,
      memberNumber: null,
    });
  });

  it('asks for the member number once a health insurance is chosen', async () => {
    fillIdentity();
    setValue('#healthInsuranceId', 'hi-1');
    await fixture.whenStable();
    submit();
    await fixture.whenStable();

    expect(saved).toBeUndefined();
    expect(element.textContent).toContain('Member number is required');

    setValue('#memberNumber', '210-555');
    submit();

    expect(saved?.healthInsuranceId).toBe('hi-1');
    expect(saved?.memberNumber).toBe('210-555');
  });

  it('rejects an invalid email before calling the API', async () => {
    fillIdentity();
    setValue('#email', 'juan');
    submit();
    await fixture.whenStable();

    expect(saved).toBeUndefined();
    expect(element.textContent).toContain('Enter a valid email address.');
  });
});
