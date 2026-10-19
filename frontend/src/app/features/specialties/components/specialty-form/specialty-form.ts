import { Component, inject, input, OnInit, output } from "@angular/core";
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import {
  Specialty,
  SPECIALTY_NAME_MAX_LENGTH,
  SpecialtyRequest,
} from "../../data-access/specialty.model";

@Component({
  imports: [ReactiveFormsModule],
  selector: "app-specialty-form",
  styleUrl: "./specialty-form.css",
  templateUrl: "./specialty-form.html",
})
export class SpecialtyForm implements OnInit {
  readonly specialty = input<Specialty>();
  readonly saving = input(false);

  readonly save = output<SpecialtyRequest>();
  readonly cancel = output<void>();

  protected readonly nameMaxLength = SPECIALTY_NAME_MAX_LENGTH;

  private readonly fb = inject(NonNullableFormBuilder);

  protected readonly form = this.fb.group({
    name: [
      "",
      [Validators.required, Validators.maxLength(SPECIALTY_NAME_MAX_LENGTH)],
    ],
  });

  ngOnInit(): void {
    const specialty = this.specialty();
    if (specialty) {
      this.form.patchValue(specialty);
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
