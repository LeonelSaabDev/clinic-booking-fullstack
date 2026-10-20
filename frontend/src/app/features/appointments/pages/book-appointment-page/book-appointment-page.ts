import { HttpErrorResponse } from "@angular/common/http";
import { Component, computed, inject, linkedSignal, signal } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { AvailableSlot } from "../../../doctors/data-access/doctor-schedule.model";
import { DoctorsApi } from "../../../doctors/data-access/doctors-api";
import { HealthInsurancesApi } from "../../../health-insurances/data-access/health-insurances-api";
import { Patient } from "../../../patients/data-access/patient.model";
import { PatientsApi } from "../../../patients/data-access/patients-api";
import { toClinicDate, toClinicTime, tomorrowAsIsoDate } from "../../../../shared/clinic-time";
import { SlotPicker } from "../../components/slot-picker/slot-picker";
import { coverageOptionFor } from "../../data-access/appointment.model";
import { AppointmentsApi } from "../../data-access/appointments-api";

@Component({
  imports: [SlotPicker, RouterLink],
  selector: "app-book-appointment-page",
  styleUrl: "./book-appointment-page.css",
  templateUrl: "./book-appointment-page.html",
})
export class BookAppointmentPage {
  private readonly appointmentsApi = inject(AppointmentsApi);
  private readonly doctorsApi = inject(DoctorsApi);
  private readonly patientsApi = inject(PatientsApi);
  private readonly healthInsurancesApi = inject(HealthInsurancesApi);
  private readonly router = inject(Router);

  protected readonly doctors = rxResource({ stream: () => this.doctorsApi.getAll() });
  private readonly healthInsurances = rxResource({ stream: () => this.healthInsurancesApi.getAll() });

  protected readonly doctorId = signal("");
  protected readonly date = signal(tomorrowAsIsoDate());

  protected readonly selectedDoctor = computed(
    () => this.doctors.value()?.find((doctor) => doctor.id === this.doctorId()) ?? null,
  );

  protected readonly availability = rxResource({
    params: () => (this.doctorId() && this.date() ? { id: this.doctorId(), date: this.date() } : undefined),
    stream: ({ params }) => this.doctorsApi.getAvailability(params.id, params.date),
  });

  /** Resets automatically whenever the doctor or the day changes, so a stale slot can never be booked. */
  protected readonly selectedSlot = linkedSignal<{ doctorId: string; date: string }, AvailableSlot | null>({
    source: () => ({ doctorId: this.doctorId(), date: this.date() }),
    computation: () => null,
  });

  protected readonly patientTerm = signal("");
  protected readonly patientResults = rxResource({
    params: () => this.patientTerm() || undefined,
    stream: ({ params: term }) => this.patientsApi.search(term),
  });
  protected readonly selectedPatient = signal<Patient | null>(null);

  protected readonly coverage = computed(() => {
    const doctor = this.selectedDoctor();
    const patient = this.selectedPatient();
    return doctor && patient ? coverageOptionFor(doctor, patient, this.healthInsurances.value() ?? []) : null;
  });

  /** Defaults to using the health insurance whenever the doctor accepts it. */
  protected readonly useHealthInsurance = linkedSignal(() => this.coverage()?.kind === "accepted");

  protected readonly booking = signal(false);
  protected readonly serverError = signal<string | null>(null);

  protected readonly canBook = computed(
    () => !!this.selectedDoctor() && !!this.selectedSlot() && !!this.selectedPatient() && !this.booking(),
  );

  protected readonly clinicTime = toClinicTime;

  protected selectDoctor(id: string): void {
    this.doctorId.set(id);
    this.serverError.set(null);
  }

  protected selectDate(value: string): void {
    this.date.set(value);
    this.serverError.set(null);
  }

  protected searchPatients(term: string): void {
    this.patientTerm.set(term.trim());
  }

  protected choosePatient(patient: Patient): void {
    this.selectedPatient.set(patient);
    this.serverError.set(null);
  }

  protected book(): void {
    const doctor = this.selectedDoctor();
    const slot = this.selectedSlot();
    const patient = this.selectedPatient();
    if (!doctor || !slot || !patient) {
      return;
    }

    this.booking.set(true);
    this.serverError.set(null);

    this.appointmentsApi
      .book({
        doctorId: doctor.id,
        patientId: patient.id,
        startsAt: slot.startsAt,
        useHealthInsurance: this.useHealthInsurance(),
      })
      .subscribe({
        next: () =>
          this.router.navigate(["/appointments"], { queryParams: { date: toClinicDate(slot.startsAt) } }),
        error: (error: HttpErrorResponse) => {
          this.booking.set(false);
          this.serverError.set(error.error?.detail ?? "The appointment could not be booked.");

          if (error.status === 409) {
            // Someone took the slot in the meantime: refresh what is really free.
            this.selectedSlot.set(null);
            this.availability.reload();
          }
        },
      });
  }
}
