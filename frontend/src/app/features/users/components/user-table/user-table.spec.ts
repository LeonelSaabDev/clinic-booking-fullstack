import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserTable } from './user-table';

describe('UserTable', () => {
  let fixture: ComponentFixture<UserTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserTable],
    }).compileComponents();

    fixture = TestBed.createComponent(UserTable);
  });

  it('lists each account with its role and doctor', async () => {
    fixture.componentRef.setInput('users', [
      { id: 'u-1', email: 'ana@clinica.com', role: 'Doctor', doctorName: 'García, Ana' },
      { id: 'u-2', email: 'recepcion@clinica.com', role: 'Receptionist', doctorName: null },
    ]);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent;
    expect(text).toContain('ana@clinica.com');
    expect(text).toContain('García, Ana');
    expect(text).toContain('Receptionist');
  });
});
