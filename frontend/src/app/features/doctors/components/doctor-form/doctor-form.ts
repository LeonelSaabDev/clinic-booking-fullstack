import { Component, inject, input, OnInit, output } from "@angular/core";
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { HealthInsurance } from "../../../health-insurances/data-access/health-insurance.model";
import { Specialty } from "../../../specialties/data-access/specialty.model";
import {
  Doctor,
  DOCTOR_LICENSE_NUMBER_MAX_LENGTH,
  DOCTOR_NAME_MAX_LENGTH,
  DoctorRequest,
} from "../../data-access/doctor.model";

@Component({
  imports: [ReactiveFormsModule],
  selector: "app-doctor-form",
  styleUrl: "./doctor-form.css",
  templateUrl: "./doctor-form.html",
})
export class DoctorForm implements OnInit {
  readonly doctor = input<Doctor>();
  readonly specialties = input.required<Specialty[]>();
  readonly healthInsurances = input.required<HealthInsurance[]>();
  readonly saving = input(false);

  readonly save = output<DoctorRequest>();
  readonly cancel = output<void>();

  protected readonly nameMaxLength = DOCTOR_NAME_MAX_LENGTH;
  protected readonly licenseNumberMaxLength = DOCTOR_LICENSE_NUMBER_MAX_LENGTH;

  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form = this.fb.group({
    firstName: ["", [Validators.required, Validators.maxLength(DOCTOR_NAME_MAX_LENGTH)]],
    lastName: ["", [Validators.required, Validators.maxLength(DOCTOR_NAME_MAX_LENGTH)]],
    licenseNumber: [
      "",
      [Validators.required, Validators.maxLength(DOCTOR_LICENSE_NUMBER_MAX_LENGTH)],
    ],
    specialtyId: ["", Validators.required],
    healthInsuranceIds: this.fb.control<string[]>([]),
  });

  ngOnInit(): void {
    const doctor = this.doctor();
    if (doctor) {
      this.form.patchValue(doctor);
    }
  }

  protected isAccepted(healthInsuranceId: string): boolean {
    return this.form.controls.healthInsuranceIds.value.includes(healthInsuranceId);
  }

  protected toggleHealthInsurance(healthInsuranceId: string, accepted: boolean): void {
    const control = this.form.controls.healthInsuranceIds;
    const others = control.value.filter((id) => id !== healthInsuranceId);

    control.setValue(accepted ? [...others, healthInsuranceId] : others);
    control.markAsDirty();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.save.emit(this.form.getRawValue());
  }
}
