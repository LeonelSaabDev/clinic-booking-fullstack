import { Component, input, output } from "@angular/core";
import { HealthInsurance } from "../../data-access/health-insurance.model";

@Component({
  selector: "app-health-insurance-table",
  styleUrl: "./health-insurance-table.css",
  templateUrl: "./health-insurance-table.html",
})
export class HealthInsuranceTable {
  readonly healthInsurances = input.required<HealthInsurance[]>();
  readonly rename = output<HealthInsurance>();
  readonly delete = output<HealthInsurance>();
}
