import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CreateUserRequest } from '../../data-access/user.model';
import { UserForm } from './user-form';

describe('UserForm', () => {
  let fixture: ComponentFixture<UserForm>;
  let element: HTMLElement;
  let saved: CreateUserRequest | undefined;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserForm],
    }).compileComponents();

    fixture = TestBed.createComponent(UserForm);
    fixture.componentRef.setInput('availableDoctors', [
      { id: 'doc-1', firstName: 'Ana', lastName: 'García', licenseNumber: 'MN', specialtyId: 's', healthInsuranceIds: [] },
    ]);
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

  function submit(): void {
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
  }

  it('creates a doctor account linked to the chosen doctor', () => {
    setValue('#doctorId', 'doc-1');
    setValue('#email', 'ana@clinica.com');
    setValue('#password', 'a long enough phrase');
    submit();

    expect(saved).toEqual({ email: 'ana@clinica.com', password: 'a long enough phrase', role: 'Doctor', doctorId: 'doc-1' });
  });

  it('requires choosing the doctor for a doctor account', async () => {
    setValue('#email', 'ana@clinica.com');
    setValue('#password', 'a long enough phrase');
    submit();
    await fixture.whenStable();

    expect(saved).toBeUndefined();
    expect(element.textContent).toContain('Choose the doctor this account belongs to.');
  });

  it('creates a receptionist without a doctor', async () => {
    setValue('#role', 'Receptionist');
    await fixture.whenStable();
    setValue('#email', 'recepcion@clinica.com');
    setValue('#password', 'a long enough phrase');
    submit();

    expect(saved?.role).toBe('Receptionist');
    expect(saved?.doctorId).toBeNull();
  });

  it('rejects short passwords before calling the API', async () => {
    setValue('#doctorId', 'doc-1');
    setValue('#email', 'ana@clinica.com');
    setValue('#password', 'short');
    submit();
    await fixture.whenStable();

    expect(saved).toBeUndefined();
  });
});
