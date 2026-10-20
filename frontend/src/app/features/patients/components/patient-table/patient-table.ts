import { Component, input, output } from "@angular/core";
import { PatientRow } from "../../data-access/patient.model";

@Component({
  selector: "app-patient-table",
  styleUrl: "./patient-table.css",
  templateUrl: "./patient-table.html",
})
export class PatientTable {
  readonly patients = input.required<PatientRow[]>();
  readonly edit = output<PatientRow>();
  readonly delete = output<PatientRow>();
}
