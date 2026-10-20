import { HttpErrorResponse } from "@angular/common/http";
import { Component, inject, input, signal } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { ScheduleEditor } from "../../components/schedule-editor/schedule-editor";
import {
  DoctorSchedule,
  toClinicTime,
  tomorrowAsIsoDate,
} from "../../data-access/doctor-schedule.model";
import { DoctorsApi } from "../../data-access/doctors-api";

@Component({
  imports: [ScheduleEditor, RouterLink],
  selector: "app-doctor-schedule-page",
  styleUrl: "./doctor-schedule-page.css",
  templateUrl: "./doctor-schedule-page.html",
})
export class DoctorSchedulePage {
  private readonly api = inject(DoctorsApi);
  private readonly router = inject(Router);

  /** Bound from the `:id` route parameter. */
  readonly id = input.required<string>();

  protected readonly doctor = rxResource({
    params: () => this.id(),
    stream: ({ params: id }) => this.api.getById(id),
  });

  protected readonly schedule = rxResource({
    params: () => this.id(),
    stream: ({ params: id }) => this.api.getSchedule(id),
  });

  protected readonly date = signal(tomorrowAsIsoDate());

  protected readonly availability = rxResource({
    // An empty date input yields "", which would be a 400; undefined keeps the resource idle.
    params: () => (this.date() ? { id: this.id(), date: this.date() } : undefined),
    stream: ({ params }) => this.api.getAvailability(params.id, params.date),
  });

  protected readonly saving = signal(false);
  protected readonly serverError = signal<string | null>(null);
  protected readonly saved = signal(false);

  protected readonly clinicTime = toClinicTime;

  protected save(schedule: DoctorSchedule): void {
    this.saving.set(true);
    this.saved.set(false);
    this.serverError.set(null);

    this.api.updateSchedule(this.id(), schedule).subscribe({
      next: () => {
        this.saving.set(false);
        this.saved.set(true);
        this.availability.reload();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.serverError.set(error.error?.detail ?? "The schedule could not be saved.");
      },
    });
  }

  protected changeDate(value: string): void {
    this.date.set(value);
  }

  protected back(): void {
    this.router.navigate(["/doctors"]);
  }
}
