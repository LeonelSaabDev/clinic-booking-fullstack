import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AvailableSlot } from '../../../doctors/data-access/doctor-schedule.model';
import { SlotPicker } from './slot-picker';

describe('SlotPicker', () => {
  let fixture: ComponentFixture<SlotPicker>;

  const nine: AvailableSlot = { startsAt: '2026-10-21T09:00:00-03:00', endsAt: '2026-10-21T09:30:00-03:00' };
  const nineThirty: AvailableSlot = { startsAt: '2026-10-21T09:30:00-03:00', endsAt: '2026-10-21T10:00:00-03:00' };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SlotPicker],
    }).compileComponents();

    fixture = TestBed.createComponent(SlotPicker);
  });

  async function render(slots: AvailableSlot[], selected: AvailableSlot | null = null): Promise<HTMLElement> {
    fixture.componentRef.setInput('slots', slots);
    fixture.componentRef.setInput('selected', selected);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('suggests another date when there are no free slots', async () => {
    expect((await render([])).textContent).toContain('No free slots on this day.');
  });

  it('marks the selected slot for assistive technology', async () => {
    const element = await render([nine, nineThirty], nineThirty);

    const checked = element.querySelector('[aria-checked="true"]');
    expect(checked?.textContent?.trim()).toBe('09:30');
  });

  it('emits the clicked slot', async () => {
    const element = await render([nine, nineThirty]);
    let picked: AvailableSlot | undefined;
    fixture.componentInstance.select.subscribe((slot) => (picked = slot));

    element.querySelector<HTMLButtonElement>('.slot')!.click();

    expect(picked).toEqual(nine);
  });
});
