import { HttpErrorResponse } from "@angular/common/http";
import { Component, computed, inject, signal } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { HealthInsurancesApi } from "../../../health-insurances/data-access/health-insurances-api";
import { PatientTable } from "../../components/patient-table/patient-table";
import { PatientRow, toPatientRows } from "../../data-access/patient.model";
import { PatientsApi } from "../../data-access/patients-api";

@Component({
  imports: [PatientTable, RouterLink],
  selector: "app-patient-list-page",
  styleUrl: "./patient-list-page.css",
  templateUrl: "./patient-list-page.html",
})
export class PatientListPage {
  private readonly patientsApi = inject(PatientsApi);
  private readonly healthInsurancesApi = inject(HealthInsurancesApi);
  private readonly router = inject(Router);

  protected readonly searchTerm = signal("");

  private readonly patients = rxResource({
    params: () => this.searchTerm(),
    stream: ({ params: term }) => this.patientsApi.search(term),
  });

  private readonly healthInsurances = rxResource({ stream: () => this.healthInsurancesApi.getAll() });

  protected readonly isLoading = computed(
    () => this.patients.isLoading() || this.healthInsurances.isLoading(),
  );

  protected readonly hasError = computed(() => !!(this.patients.error() || this.healthInsurances.error()));

  protected readonly rows = computed(() =>
    toPatientRows(this.patients.value() ?? [], this.healthInsurances.value() ?? []),
  );

  protected search(term: string): void {
    this.searchTerm.set(term.trim());
  }

  protected editPatient(patient: PatientRow): void {
    this.router.navigate(["/patients", patient.id, "edit"]);
  }

  protected deletePatient(patient: PatientRow): void {
    if (!confirm(`Delete "${patient.fullName}"?`)) {
      return;
    }

    this.patientsApi.delete(patient.id).subscribe({
      next: () => this.patients.reload(),
      error: (error: HttpErrorResponse) =>
        alert(error.error?.detail ?? "The patient could not be deleted. Please try again."),
    });
  }
}
