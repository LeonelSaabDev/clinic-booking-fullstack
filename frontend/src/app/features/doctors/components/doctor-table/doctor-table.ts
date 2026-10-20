import { Component, input, output } from "@angular/core";
import { DoctorRow } from "../../data-access/doctor.model";

@Component({
  selector: "app-doctor-table",
  styleUrl: "./doctor-table.css",
  templateUrl: "./doctor-table.html",
})
export class DoctorTable {
  readonly doctors = input.required<DoctorRow[]>();
  readonly edit = output<DoctorRow>();
  readonly schedule = output<DoctorRow>();
  readonly delete = output<DoctorRow>();
}
