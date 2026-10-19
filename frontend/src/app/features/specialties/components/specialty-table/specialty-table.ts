import { Component, input, output } from "@angular/core";
import { Specialty } from "../../data-access/specialty.model";

@Component({
  selector: "app-specialty-table",
  styleUrl: "./specialty-table.css",
  templateUrl: "./specialty-table.html",
})
export class SpecialtyTable {
  readonly specialties = input.required<Specialty[]>();
  readonly rename = output<Specialty>();
  readonly delete = output<Specialty>();
}
