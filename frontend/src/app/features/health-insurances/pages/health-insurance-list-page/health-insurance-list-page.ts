import { HttpErrorResponse } from "@angular/common/http";
import { Component, inject } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { HealthInsuranceTable } from "../../components/health-insurance-table/health-insurance-table";
import { HealthInsurance } from "../../data-access/health-insurance.model";
import { HealthInsurancesApi } from "../../data-access/health-insurances-api";

@Component({
  imports: [HealthInsuranceTable, RouterLink],
  selector: "app-health-insurance-list-page",
  styleUrl: "./health-insurance-list-page.css",
  templateUrl: "./health-insurance-list-page.html",
})
export class HealthInsuranceListPage {
  private readonly api = inject(HealthInsurancesApi);
  private readonly router = inject(Router);

  protected readonly healthInsurances = rxResource({
    stream: () => this.api.getAll(),
  });

  protected renameHealthInsurance(healthInsurance: HealthInsurance): void {
    this.router.navigate(["/health-insurances", healthInsurance.id, "edit"]);
  }

  protected deleteHealthInsurance(healthInsurance: HealthInsurance): void {
    if (!confirm(`Delete "${healthInsurance.name}"?`)) {
      return;
    }

    this.api.delete(healthInsurance.id).subscribe({
      next: () => this.healthInsurances.reload(),
      error: (error: HttpErrorResponse) =>
        alert(error.error?.detail ?? "The health insurance could not be deleted. Please try again."),
    });
  }
}
