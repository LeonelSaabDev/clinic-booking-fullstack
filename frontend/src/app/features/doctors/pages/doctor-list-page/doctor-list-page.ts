import { Component, computed, inject } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { HealthInsurancesApi } from "../../../health-insurances/data-access/health-insurances-api";
import { SpecialtiesApi } from "../../../specialties/data-access/specialties-api";
import { DoctorTable } from "../../components/doctor-table/doctor-table";
import { DoctorRow, toDoctorRows } from "../../data-access/doctor.model";
import { DoctorsApi } from "../../data-access/doctors-api";

@Component({
  imports: [DoctorTable, RouterLink],
  selector: "app-doctor-list-page",
  styleUrl: "./doctor-list-page.css",
  templateUrl: "./doctor-list-page.html",
})
export class DoctorListPage {
  private readonly doctorsApi = inject(DoctorsApi);
  private readonly specialtiesApi = inject(SpecialtiesApi);
  private readonly healthInsurancesApi = inject(HealthInsurancesApi);
  private readonly router = inject(Router);

  private readonly doctors = rxResource({ stream: () => this.doctorsApi.getAll() });
  private readonly specialties = rxResource({ stream: () => this.specialtiesApi.getAll() });
  private readonly healthInsurances = rxResource({ stream: () => this.healthInsurancesApi.getAll() });

  protected readonly isLoading = computed(
    () =>
      this.doctors.isLoading() || this.specialties.isLoading() || this.healthInsurances.isLoading(),
  );

  protected readonly hasError = computed(
    () => !!(this.doctors.error() || this.specialties.error() || this.healthInsurances.error()),
  );

  protected readonly rows = computed(() =>
    toDoctorRows(
      this.doctors.value() ?? [],
      this.specialties.value() ?? [],
      this.healthInsurances.value() ?? [],
    ),
  );

  protected openSchedule(doctor: DoctorRow): void {
    this.router.navigate(["/doctors", doctor.id, "schedule"]);
  }

  protected editDoctor(doctor: DoctorRow): void {
    this.router.navigate(["/doctors", doctor.id, "edit"]);
  }

  protected deleteDoctor(doctor: DoctorRow): void {
    if (!confirm(`Delete "${doctor.fullName}"?`)) {
      return;
    }

    this.doctorsApi.delete(doctor.id).subscribe({
      next: () => this.doctors.reload(),
      error: () => alert("The doctor could not be deleted. Please try again."),
    });
  }
}
