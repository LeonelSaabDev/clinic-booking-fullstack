import { HttpErrorResponse } from "@angular/common/http";
import { Component, computed, inject, input, signal } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { HealthInsuranceForm } from "../../components/health-insurance-form/health-insurance-form";
import { HealthInsuranceRequest } from "../../data-access/health-insurance.model";
import { HealthInsurancesApi } from "../../data-access/health-insurances-api";

@Component({
  imports: [HealthInsuranceForm, RouterLink],
  selector: "app-health-insurance-form-page",
  styleUrl: "./health-insurance-form-page.css",
  templateUrl: "./health-insurance-form-page.html",
})
export class HealthInsuranceFormPage {
  private readonly api = inject(HealthInsurancesApi);
  private readonly router = inject(Router);

  /** Bound from the `:id` route parameter. Undefined when creating a health insurance. */
  readonly id = input<string>();

  protected readonly isEditMode = computed(() => this.id() !== undefined);

  protected readonly healthInsurance = rxResource({
    params: () => this.id(),
    stream: ({ params: id }) => this.api.getById(id),
  });

  protected readonly saving = signal(false);
  protected readonly serverError = signal<string | null>(null);

  protected save(request: HealthInsuranceRequest): void {
    this.saving.set(true);
    this.serverError.set(null);

    const id = this.id();
    const request$ = id
      ? this.api.rename(id, request)
      : this.api.create(request);

    request$.subscribe({
      next: () => this.router.navigate(["/health-insurances"]),
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.serverError.set(
          error.error?.detail ?? "The health insurance could not be saved.",
        );
      },
    });
  }

  protected cancel(): void {
    this.router.navigate(["/health-insurances"]);
  }
}
