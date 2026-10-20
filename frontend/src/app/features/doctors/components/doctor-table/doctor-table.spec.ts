import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DoctorRow } from '../../data-access/doctor.model';
import { DoctorTable } from './doctor-table';

describe('DoctorTable', () => {
  let fixture: ComponentFixture<DoctorTable>;

  const ana: DoctorRow = {
    id: 'doc-1',
    fullName: 'García, Ana',
    licenseNumber: 'MN 12345',
    specialtyName: 'Cardiology',
    healthInsuranceNames: ['OSDE'],
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DoctorTable],
    }).compileComponents();

    fixture = TestBed.createComponent(DoctorTable);
  });

  async function render(doctors: DoctorRow[]): Promise<HTMLElement> {
    fixture.componentRef.setInput('doctors', doctors);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('shows the empty state when there are no doctors', async () => {
    const element = await render([]);

    expect(element.textContent).toContain('No doctors yet.');
  });

  it('shows each doctor with specialty and accepted health insurances', async () => {
    const element = await render([ana]);

    expect(element.textContent).toContain('García, Ana');
    expect(element.textContent).toContain('MN 12345');
    expect(element.textContent).toContain('Cardiology');
    expect(element.textContent).toContain('OSDE');
  });

  it('labels a doctor without health insurances as private only', async () => {
    const element = await render([{ ...ana, healthInsuranceNames: [] }]);

    expect(element.textContent).toContain('Private only');
  });

  it('emits the doctor when its delete button is clicked', async () => {
    const element = await render([ana]);
    let deleted: DoctorRow | undefined;
    fixture.componentInstance.delete.subscribe((doctor) => (deleted = doctor));

    const deleteButton = [...element.querySelectorAll('button')].find(
      (button) => button.textContent?.trim() === 'Delete',
    );
    deleteButton!.click();

    expect(deleted).toEqual(ana);
  });
});
