export const HEALTH_INSURANCE_NAME_MAX_LENGTH = 100;

export interface HealthInsurance {
  id: string;
  name: string;
}

export type HealthInsuranceRequest = Omit<HealthInsurance, "id">;
