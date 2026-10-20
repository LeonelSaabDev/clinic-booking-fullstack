import { Component, inject, input, output } from "@angular/core";
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from "@angular/forms";
import { UserRole } from "../../../../core/auth/auth.model";
import { Doctor } from "../../../doctors/data-access/doctor.model";
import { CreateUserRequest, USER_PASSWORD_MIN_LENGTH } from "../../data-access/user.model";

function doctorWhenDoctorRole(group: AbstractControl): ValidationErrors | null {
  const { role, doctorId } = group.value as { role: UserRole; doctorId: string };
  return role === "Doctor" && !doctorId ? { doctorRequired: true } : null;
}

@Component({
  imports: [ReactiveFormsModule],
  selector: "app-user-form",
  styleUrl: "./user-form.css",
  templateUrl: "./user-form.html",
})
export class UserForm {
  /** Only doctors that don't have an account yet. */
  readonly availableDoctors = input.required<Doctor[]>();
  readonly saving = input(false);
  readonly save = output<CreateUserRequest>();

  protected readonly minPasswordLength = USER_PASSWORD_MIN_LENGTH;

  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form = this.fb.group(
    {
      email: ["", [Validators.required, Validators.email]],
      password: ["", [Validators.required, Validators.minLength(USER_PASSWORD_MIN_LENGTH)]],
      role: this.fb.control<UserRole>("Doctor"),
      doctorId: [""],
    },
    { validators: doctorWhenDoctorRole },
  );

  protected get isDoctorRole(): boolean {
    return this.form.controls.role.value === "Doctor";
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, password, role, doctorId } = this.form.getRawValue();
    this.save.emit({ email, password, role, doctorId: role === "Doctor" ? doctorId : null });
  }

  /** Called by the page after a successful save. */
  reset(): void {
    this.form.reset({ email: "", password: "", role: "Doctor", doctorId: "" });
  }
}
