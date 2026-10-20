import { HealthInsurance } from "../../health-insurances/data-access/health-insurance.model";

export const PATIENT_NAME_MAX_LENGTH = 100;
export const PATIENT_DOCUMENT_NUMBER_PATTERN = /^[\d.\s-]{6,13}$/;
export const PATIENT_PHONE_MAX_LENGTH = 30;
export const PATIENT_MEMBER_NUMBER_MAX_LENGTH = 30;

export interface Patient {
  id: string;
  firstName: string;
  lastName: string;
  documentNumber: string;
  email: string;
  phone: string;
  healthInsuranceId: string | null;
  memberNumber: string | null;
}

export type PatientRequest = Omit<Patient, "id">;

/** A patient ready to be displayed, with the health insurance id resolved to a name. */
export interface PatientRow {
  id: string;
  fullName: string;
  documentNumber: string;
  email: string;
  phone: string;
  /** e.g. "OSDE · 210-555", or null for private patients. */
  coverage: string | null;
}

export function toPatientRows(patients: Patient[], healthInsurances: HealthInsurance[]): PatientRow[] {
  const healthInsuranceNames = new Map(healthInsurances.map((h) => [h.id, h.name]));

  return patients.map((patient) => ({
    id: patient.id,
    fullName: `${patient.lastName}, ${patient.firstName}`,
    documentNumber: patient.documentNumber,
    email: patient.email,
    phone: patient.phone,
    coverage: patient.healthInsuranceId
      ? `${healthInsuranceNames.get(patient.healthInsuranceId) ?? "Unknown health insurance"} · ${patient.memberNumber}`
      : null,
  }));
}
