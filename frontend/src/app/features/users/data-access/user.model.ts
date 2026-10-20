import { UserRole } from "../../../core/auth/auth.model";
import { Doctor } from "../../doctors/data-access/doctor.model";

/** Mirrors the API: length matters more than symbol rules. */
export const USER_PASSWORD_MIN_LENGTH = 10;

export interface User {
  id: string;
  email: string;
  role: UserRole;
  doctorId: string | null;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  role: UserRole;
  doctorId: string | null;
}

export interface UserRow {
  id: string;
  email: string;
  role: UserRole;
  doctorName: string | null;
}

export function toUserRows(users: User[], doctors: Doctor[]): UserRow[] {
  const doctorNames = new Map(doctors.map((d) => [d.id, `${d.lastName}, ${d.firstName}`]));
  return users.map((user) => ({
    id: user.id,
    email: user.email,
    role: user.role,
    doctorName: user.doctorId ? (doctorNames.get(user.doctorId) ?? "Unknown doctor") : null,
  }));
}

/** Doctors that can still get an account: one account per doctor. */
export function doctorsWithoutAccount(doctors: Doctor[], users: User[]): Doctor[] {
  const linked = new Set(users.map((u) => u.doctorId).filter((id): id is string => id !== null));
  return doctors.filter((doctor) => !linked.has(doctor.id));
}
