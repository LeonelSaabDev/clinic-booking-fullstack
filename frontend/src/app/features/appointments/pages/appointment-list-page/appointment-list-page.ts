import { Component, computed, inject, input, linkedSignal, signal } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { DoctorsApi } from "../../../doctors/data-access/doctors-api";
import { HealthInsurancesApi } from "../../../health-insurances/data-access/health-insurances-api";
import { PatientsApi } from "../../../patients/data-access/patients-api";
import { todayAsIsoDate } from "../../../../shared/clinic-time";
import { AppointmentTable } from "../../components/appointment-table/appointment-table";
import { toAppointmentRows } from "../../data-access/appointment.model";
import { AppointmentsApi } from "../../data-access/appointments-api";

@Component({
  imports: [AppointmentTable, RouterLink],
  selector: "app-appointment-list-page",
  styleUrl: "./appointment-list-page.css",
  templateUrl: "./appointment-list-page.html",
})
export class AppointmentListPage {
  private readonly appointmentsApi = inject(AppointmentsApi);
  private readonly doctorsApi = inject(DoctorsApi);
  private readonly patientsApi = inject(PatientsApi);
  private readonly healthInsurancesApi = inject(HealthInsurancesApi);
  private readonly router = inject(Router);

  /** Bound from the `?date=` query parameter, so a booking can return to the right day. */
  readonly date = input<string>();

  protected readonly selectedDate = linkedSignal(() => this.date() || todayAsIsoDate());
  protected readonly doctorFilter = signal("");

  private readonly appointments = rxResource({
    params: () => ({ date: this.selectedDate(), doctorId: this.doctorFilter() }),
    stream: ({ params }) => this.appointmentsApi.list(params.date, params.doctorId || undefined),
  });

  protected readonly doctors = rxResource({ stream: () => this.doctorsApi.getAll() });
  private readonly patients = rxResource({ stream: () => this.patientsApi.search() });
  private readonly healthInsurances = rxResource({ stream: () => this.healthInsurancesApi.getAll() });

  protected readonly isLoading = computed(
    () =>
      this.appointments.isLoading() ||
      this.doctors.isLoading() ||
      this.patients.isLoading() ||
      this.healthInsurances.isLoading(),
  );

  protected readonly hasError = computed(
    () =>
      !!(this.appointments.error() || this.doctors.error() || this.patients.error() || this.healthInsurances.error()),
  );

  protected readonly rows = computed(() =>
    toAppointmentRows(
      this.appointments.value() ?? [],
      this.doctors.value() ?? [],
      this.patients.value() ?? [],
      this.healthInsurances.value() ?? [],
    ),
  );

  protected changeDate(value: string): void {
    if (value) {
      this.selectedDate.set(value);
      this.router.navigate([], { queryParams: { date: value }, replaceUrl: true });
    }
  }
}
