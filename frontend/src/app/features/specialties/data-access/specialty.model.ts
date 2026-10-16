export const SPECIALTY_NAME_MAX_LENGHT = 100;

export interface Specialty {
  id: string;
  name: string;
}

export type SpecialtyRequest = Omit<Specialty, "id">;
