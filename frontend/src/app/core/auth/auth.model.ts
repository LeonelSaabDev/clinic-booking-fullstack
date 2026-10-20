export type UserRole = "Receptionist" | "Doctor";

export interface LoginRequest {
  email: string;
  password: string;
}

/** What the API returns on sign-in, and what the app keeps for the browser tab. */
export interface Session {
  accessToken: string;
  /** ISO 8601 instant. */
  expiresAt: string;
  email: string;
  role: UserRole;
  /** Set only for doctors: their own doctor record. */
  doctorId: string | null;
}
