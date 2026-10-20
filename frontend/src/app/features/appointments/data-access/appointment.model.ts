import { Doctor } from "../../doctors/data-access/doctor.model";
import { HealthInsurance } from "../../health-insurances/data-access/health-insurance.model";
import { Patient } from "../../patients/data-access/patient.model";
import { toClinicTime } from "../../../shared/clinic-time";

export type AppointmentStatus = "Scheduled" | "Completed" | "Cancelled" | "NoShow";

export interface Appointment {
  id: string;
  doctorId: string;
  patientId: string;
  /** ISO 8601 in clinic time, e.g. "2026-10-21T09:00:00-03:00". */
  startsAt: string;
  endsAt: string;
  status: AppointmentStatus;
  healthInsuranceId: string | null;
  memberNumber: string | null;
}

export interface BookAppointmentRequest {
  doctorId: string;
  patientId: string;
  startsAt: string;
  useHealthInsurance: boolean;
}

export interface AppointmentRow {
  id: string;
  time: string;
  doctorName: string;
  patientName: string;
  documentNumber: string;
  coverage: string;
  status: AppointmentStatus;
}

export function toAppointmentRows(
  appointments: Appointment[],
  doctors: Doctor[],
  patients: Patient[],
  healthInsurances: HealthInsurance[],
): AppointmentRow[] {
  const doctorsById = new Map(doctors.map((d) => [d.id, d]));
  const patientsById = new Map(patients.map((p) => [p.id, p]));
  const healthInsuranceNames = new Map(healthInsurances.map((h) => [h.id, h.name]));

  return appointments.map((appointment) => {
    const doctor = doctorsById.get(appointment.doctorId);
    const patient = patientsById.get(appointment.patientId);

    return {
      id: appointment.id,
      time: `${toClinicTime(appointment.startsAt)}–${toClinicTime(appointment.endsAt)}`,
      doctorName: doctor ? `${doctor.lastName}, ${doctor.firstName}` : "Unknown doctor",
      patientName: patient ? `${patient.lastName}, ${patient.firstName}` : "Unknown patient",
      documentNumber: patient?.documentNumber ?? "",
      coverage: appointment.healthInsuranceId
        ? `${healthInsuranceNames.get(appointment.healthInsuranceId) ?? "Health insurance"} · ${appointment.memberNumber}`
        : "Private",
      status: appointment.status,
    };
  });
}

export type CoverageOption =
  | { kind: "private-only" }
  | { kind: "accepted"; healthInsuranceName: string; memberNumber: string }
  | { kind: "not-accepted"; healthInsuranceName: string };

/** What coverage can be offered when a given patient books with a given doctor. */
export function coverageOptionFor(
  doctor: Doctor,
  patient: Patient,
  healthInsurances: HealthInsurance[],
): CoverageOption {
  if (!patient.healthInsuranceId) {
    return { kind: "private-only" };
  }

  const healthInsuranceName =
    healthInsurances.find((h) => h.id === patient.healthInsuranceId)?.name ?? "their health insurance";

  return doctor.healthInsuranceIds.includes(patient.healthInsuranceId)
    ? { kind: "accepted", healthInsuranceName, memberNumber: patient.memberNumber ?? "" }
    : { kind: "not-accepted", healthInsuranceName };
}
