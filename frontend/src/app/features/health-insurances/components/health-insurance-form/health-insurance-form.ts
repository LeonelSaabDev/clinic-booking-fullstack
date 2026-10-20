import { Component, inject, input, OnInit, output } from "@angular/core";
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import {
  HEALTH_INSURANCE_NAME_MAX_LENGTH,
  HealthInsurance,
  HealthInsuranceRequest,
} from "../../data-access/health-insurance.model";

@Component({
  imports: [ReactiveFormsModule],
  selector: "app-health-insurance-form",
  styleUrl: "./health-insurance-form.css",
  templateUrl: "./health-insurance-form.html",
})
export class HealthInsuranceForm implements OnInit {
  readonly healthInsurance = input<HealthInsurance>();
  readonly saving = input(false);

  readonly save = output<HealthInsuranceRequest>();
  readonly cancel = output<void>();

  protected readonly nameMaxLength = HEALTH_INSURANCE_NAME_MAX_LENGTH;

  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form = this.fb.group({
    name: [
      "",
      [
        Validators.required,
        Validators.maxLength(HEALTH_INSURANCE_NAME_MAX_LENGTH),
      ],
    ],
  });

  ngOnInit(): void {
    const healthInsurance = this.healthInsurance();
    if (healthInsurance) {
      this.form.patchValue(healthInsurance);
    }
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { name } = this.form.getRawValue();
    this.save.emit({ name });
  }
}
