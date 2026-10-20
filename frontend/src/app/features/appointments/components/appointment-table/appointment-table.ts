import { Component, input } from "@angular/core";
import { AppointmentRow } from "../../data-access/appointment.model";

@Component({
  selector: "app-appointment-table",
  styleUrl: "./appointment-table.css",
  templateUrl: "./appointment-table.html",
})
export class AppointmentTable {
  readonly appointments = input.required<AppointmentRow[]>();
}
