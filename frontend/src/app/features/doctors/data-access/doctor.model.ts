import { HealthInsurance } from "../../health-insurances/data-access/health-insurance.model";
import { Specialty } from "../../specialties/data-access/specialty.model";

export const DOCTOR_NAME_MAX_LENGTH = 100;
export const DOCTOR_LICENSE_NUMBER_MAX_LENGTH = 20;

export interface Doctor {
  id: string;
  firstName: string;
  lastName: string;
  licenseNumber: string;
  specialtyId: string;
  healthInsuranceIds: string[];
}

export type DoctorRequest = Omit<Doctor, "id">;

/** A doctor ready to be displayed: ids already resolved to names. */
export interface DoctorRow {
  id: string;
  fullName: string;
  licenseNumber: string;
  specialtyName: string;
  healthInsuranceNames: string[];
}

export function toDoctorRows(
  doctors: Doctor[],
  specialties: Specialty[],
  healthInsurances: HealthInsurance[],
): DoctorRow[] {
  const specialtyNames = new Map(specialties.map((s) => [s.id, s.name]));
  const healthInsuranceNames = new Map(healthInsurances.map((h) => [h.id, h.name]));

  return doctors.map((doctor) => ({
    id: doctor.id,
    fullName: `${doctor.lastName}, ${doctor.firstName}`,
    licenseNumber: doctor.licenseNumber,
    specialtyName: specialtyNames.get(doctor.specialtyId) ?? "Unknown specialty",
    healthInsuranceNames: doctor.healthInsuranceIds
      .map((id) => healthInsuranceNames.get(id))
      .filter((name): name is string => name !== undefined)
      .sort((a, b) => a.localeCompare(b)),
  }));
}
