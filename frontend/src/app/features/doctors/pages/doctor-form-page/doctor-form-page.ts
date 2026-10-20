import { HttpErrorResponse } from "@angular/common/http";
import { Component, computed, inject, input, signal } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { HealthInsurancesApi } from "../../../health-insurances/data-access/health-insurances-api";
import { SpecialtiesApi } from "../../../specialties/data-access/specialties-api";
import { DoctorForm } from "../../components/doctor-form/doctor-form";
import { DoctorRequest } from "../../data-access/doctor.model";
import { DoctorsApi } from "../../data-access/doctors-api";

@Component({
  imports: [DoctorForm, RouterLink],
  selector: "app-doctor-form-page",
  styleUrl: "./doctor-form-page.css",
  templateUrl: "./doctor-form-page.html",
})
export class DoctorFormPage {
  private readonly doctorsApi = inject(DoctorsApi);
  private readonly specialtiesApi = inject(SpecialtiesApi);
  private readonly healthInsurancesApi = inject(HealthInsurancesApi);
  private readonly router = inject(Router);

  /** Bound from the `:id` route parameter. Undefined when creating a doctor. */
  readonly id = input<string>();

  protected readonly isEditMode = computed(() => this.id() !== undefined);

  protected readonly doctor = rxResource({
    params: () => this.id(),
    stream: ({ params: id }) => this.doctorsApi.getById(id),
  });

  protected readonly specialties = rxResource({ stream: () => this.specialtiesApi.getAll() });
  protected readonly healthInsurances = rxResource({ stream: () => this.healthInsurancesApi.getAll() });

  /** The form can only be drawn once every list it needs has arrived. */
  protected readonly isReady = computed(
    () =>
      this.specialties.hasValue() &&
      this.healthInsurances.hasValue() &&
      (!this.isEditMode() || this.doctor.hasValue()),
  );

  protected readonly loadFailed = computed(
    () => !!(this.doctor.error() || this.specialties.error() || this.healthInsurances.error()),
  );

  protected readonly saving = signal(false);
  protected readonly serverError = signal<string | null>(null);

  protected save(request: DoctorRequest): void {
    this.saving.set(true);
    this.serverError.set(null);

    const id = this.id();
    const request$ = id ? this.doctorsApi.update(id, request) : this.doctorsApi.create(request);

    request$.subscribe({
      next: () => this.router.navigate(["/doctors"]),
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.serverError.set(error.error?.detail ?? "The doctor could not be saved.");
      },
    });
  }

  protected cancel(): void {
    this.router.navigate(["/doctors"]);
  }
}
