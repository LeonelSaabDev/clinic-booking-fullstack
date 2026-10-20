import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DoctorSchedule } from '../../data-access/doctor-schedule.model';
import { ScheduleEditor } from './schedule-editor';

describe('ScheduleEditor', () => {
  let fixture: ComponentFixture<ScheduleEditor>;
  let element: HTMLElement;
  let saved: DoctorSchedule | undefined;

  async function render(schedule: DoctorSchedule): Promise<void> {
    await TestBed.configureTestingModule({
      imports: [ScheduleEditor],
    }).compileComponents();

    fixture = TestBed.createComponent(ScheduleEditor);
    fixture.componentRef.setInput('schedule', schedule);
    fixture.componentInstance.save.subscribe((value) => (saved = value));
    await fixture.whenStable();
    element = fixture.nativeElement as HTMLElement;
  }

  function button(label: string): HTMLButtonElement {
    return [...element.querySelectorAll('button')].find((b) => b.textContent?.trim() === label)!;
  }

  function setValue(control: HTMLInputElement | HTMLSelectElement, value: string): void {
    control.value = value;
    control.dispatchEvent(new Event(control.tagName === 'SELECT' ? 'change' : 'input'));
  }

  beforeEach(() => (saved = undefined));

  it('loads the existing blocks, trimming seconds for the time inputs', async () => {
    await render({
      appointmentDurationMinutes: 45,
      blocks: [{ day: 'Wednesday', start: '09:00:00', end: '13:00:00' }],
    });

    const [start, end] = element.querySelectorAll<HTMLInputElement>('input[type="time"]');
    expect(start.value).toBe('09:00');
    expect(end.value).toBe('13:00');
  });

  it('shows a hint when the doctor has no working hours yet', async () => {
    await render({ appointmentDurationMinutes: 30, blocks: [] });

    expect(element.textContent).toContain('No working hours yet.');
  });

  it('emits the schedule with a newly added block', async () => {
    await render({ appointmentDurationMinutes: 30, blocks: [] });

    button('Add block').click();
    await fixture.whenStable();
    setValue(element.querySelector<HTMLSelectElement>('.block-row select')!, 'Friday');
    button('Save schedule').click();

    expect(saved).toEqual({
      appointmentDurationMinutes: 30,
      blocks: [{ day: 'Friday', start: '09:00', end: '13:00' }],
    });
  });

  it('removes a block before saving', async () => {
    await render({
      appointmentDurationMinutes: 30,
      blocks: [{ day: 'Monday', start: '09:00:00', end: '13:00:00' }],
    });

    button('Remove').click();
    await fixture.whenStable();
    button('Save schedule').click();

    expect(saved).toEqual({ appointmentDurationMinutes: 30, blocks: [] });
  });

  it('does not save a block that ends before it starts', async () => {
    await render({
      appointmentDurationMinutes: 30,
      blocks: [{ day: 'Monday', start: '09:00:00', end: '13:00:00' }],
    });

    const [, end] = element.querySelectorAll<HTMLInputElement>('input[type="time"]');
    setValue(end, '08:00');
    button('Save schedule').click();
    await fixture.whenStable();

    expect(saved).toBeUndefined();
    expect(element.textContent).toContain('The block must end after it starts.');
  });
});
