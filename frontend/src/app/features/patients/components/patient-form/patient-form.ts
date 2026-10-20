import { Component, inject, input, OnInit, output } from "@angular/core";
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from "@angular/forms";
import { HealthInsurance } from "../../../health-insurances/data-access/health-insurance.model";
import {
  Patient,
  PATIENT_DOCUMENT_NUMBER_PATTERN,
  PATIENT_MEMBER_NUMBER_MAX_LENGTH,
  PATIENT_NAME_MAX_LENGTH,
  PATIENT_PHONE_MAX_LENGTH,
  PatientRequest,
} from "../../data-access/patient.model";

/** Mirrors the domain rule: a member number is required only when a health insurance is chosen. */
function memberNumberWhenInsured(group: AbstractControl): ValidationErrors | null {
  const { healthInsuranceId, memberNumber } = group.value as {
    healthInsuranceId: string;
    memberNumber: string;
  };
  return healthInsuranceId && !memberNumber.trim() ? { memberNumberRequired: true } : null;
}

@Component({
  imports: [ReactiveFormsModule],
  selector: "app-patient-form",
  styleUrl: "./patient-form.css",
  templateUrl: "./patient-form.html",
})
export class PatientForm implements OnInit {
  readonly patient = input<Patient>();
  readonly healthInsurances = input.required<HealthInsurance[]>();
  readonly saving = input(false);

  readonly save = output<PatientRequest>();
  readonly cancel = output<void>();

  protected readonly nameMaxLength = PATIENT_NAME_MAX_LENGTH;
  protected readonly memberNumberMaxLength = PATIENT_MEMBER_NUMBER_MAX_LENGTH;

  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form = this.fb.group(
    {
      firstName: ["", [Validators.required, Validators.maxLength(PATIENT_NAME_MAX_LENGTH)]],
      lastName: ["", [Validators.required, Validators.maxLength(PATIENT_NAME_MAX_LENGTH)]],
      documentNumber: ["", [Validators.required, Validators.pattern(PATIENT_DOCUMENT_NUMBER_PATTERN)]],
      email: ["", [Validators.required, Validators.email]],
      phone: ["", [Validators.required, Validators.maxLength(PATIENT_PHONE_MAX_LENGTH)]],
      healthInsuranceId: [""],
      memberNumber: ["", Validators.maxLength(PATIENT_MEMBER_NUMBER_MAX_LENGTH)],
    },
    { validators: memberNumberWhenInsured },
  );

  ngOnInit(): void {
    const patient = this.patient();
    if (patient) {
      this.form.patchValue({
        ...patient,
        healthInsuranceId: patient.healthInsuranceId ?? "",
        memberNumber: patient.memberNumber ?? "",
      });
    }
  }

  protected get isInsured(): boolean {
    return this.form.controls.healthInsuranceId.value !== "";
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { healthInsuranceId, memberNumber, ...details } = this.form.getRawValue();
    this.save.emit({
      ...details,
      healthInsuranceId: healthInsuranceId || null,
      memberNumber: healthInsuranceId ? memberNumber : null,
    });
  }
}
