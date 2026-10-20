import { HttpErrorResponse } from "@angular/common/http";
import { Component, computed, inject, input, signal } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { HealthInsurancesApi } from "../../../health-insurances/data-access/health-insurances-api";
import { PatientForm } from "../../components/patient-form/patient-form";
import { PatientRequest } from "../../data-access/patient.model";
import { PatientsApi } from "../../data-access/patients-api";

@Component({
  imports: [PatientForm, RouterLink],
  selector: "app-patient-form-page",
  styleUrl: "./patient-form-page.css",
  templateUrl: "./patient-form-page.html",
})
export class PatientFormPage {
  private readonly patientsApi = inject(PatientsApi);
  private readonly healthInsurancesApi = inject(HealthInsurancesApi);
  private readonly router = inject(Router);

  /** Bound from the `:id` route parameter. Undefined when registering a patient. */
  readonly id = input<string>();

  protected readonly isEditMode = computed(() => this.id() !== undefined);

  protected readonly patient = rxResource({
    params: () => this.id(),
    stream: ({ params: id }) => this.patientsApi.getById(id),
  });

  protected readonly healthInsurances = rxResource({ stream: () => this.healthInsurancesApi.getAll() });

  protected readonly isReady = computed(
    () => this.healthInsurances.hasValue() && (!this.isEditMode() || this.patient.hasValue()),
  );

  protected readonly loadFailed = computed(() => !!(this.patient.error() || this.healthInsurances.error()));

  protected readonly saving = signal(false);
  protected readonly serverError = signal<string | null>(null);

  protected save(request: PatientRequest): void {
    this.saving.set(true);
    this.serverError.set(null);

    const id = this.id();
    const request$ = id ? this.patientsApi.update(id, request) : this.patientsApi.create(request);

    request$.subscribe({
      next: () => this.router.navigate(["/patients"]),
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.serverError.set(error.error?.detail ?? "The patient could not be saved.");
      },
    });
  }

  protected cancel(): void {
    this.router.navigate(["/patients"]);
  }
}
