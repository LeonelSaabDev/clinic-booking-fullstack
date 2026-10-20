import { Component, inject, input, OnInit, output } from "@angular/core";
import {
  AbstractControl,
  FormControl,
  FormGroup,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from "@angular/forms";
import {
  APPOINTMENT_DURATION_OPTIONS,
  DAYS_OF_WEEK,
  DayOfWeek,
  DoctorSchedule,
  ScheduleBlock,
  toInputTime,
} from "../../data-access/doctor-schedule.model";

type BlockForm = FormGroup<{
  day: FormControl<DayOfWeek>;
  start: FormControl<string>;
  end: FormControl<string>;
}>;

/** "HH:mm" strings compare correctly as text, so no Date parsing is needed. */
function endAfterStart(group: AbstractControl): ValidationErrors | null {
  const { start, end } = group.value as { start: string; end: string };
  return start && end && end <= start ? { endBeforeStart: true } : null;
}

@Component({
  imports: [ReactiveFormsModule],
  selector: "app-schedule-editor",
  styleUrl: "./schedule-editor.css",
  templateUrl: "./schedule-editor.html",
})
export class ScheduleEditor implements OnInit {
  readonly schedule = input.required<DoctorSchedule>();
  readonly saving = input(false);

  readonly save = output<DoctorSchedule>();
  readonly cancel = output<void>();

  protected readonly days = DAYS_OF_WEEK;
  protected readonly durations = APPOINTMENT_DURATION_OPTIONS;

  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form = this.fb.group({
    appointmentDurationMinutes: this.fb.control(30, Validators.required),
    blocks: this.fb.array<BlockForm>([]),
  });

  ngOnInit(): void {
    const schedule = this.schedule();
    this.form.controls.appointmentDurationMinutes.setValue(schedule.appointmentDurationMinutes);
    schedule.blocks.forEach((block) => this.form.controls.blocks.push(this.createBlock(block)));
  }

  protected addBlock(): void {
    this.form.controls.blocks.push(this.createBlock());
  }

  protected removeBlock(index: number): void {
    this.form.controls.blocks.removeAt(index);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.save.emit(this.form.getRawValue());
  }

  private createBlock(block?: ScheduleBlock): BlockForm {
    return this.fb.group(
      {
        day: this.fb.control<DayOfWeek>(block?.day ?? "Monday"),
        start: this.fb.control(block ? toInputTime(block.start) : "09:00", Validators.required),
        end: this.fb.control(block ? toInputTime(block.end) : "13:00", Validators.required),
      },
      { validators: endAfterStart },
    );
  }
}
