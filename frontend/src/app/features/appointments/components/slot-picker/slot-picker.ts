import { Component, input, output } from "@angular/core";
import { AvailableSlot } from "../../../doctors/data-access/doctor-schedule.model";
import { toClinicTime } from "../../../../shared/clinic-time";

@Component({
  selector: "app-slot-picker",
  styleUrl: "./slot-picker.css",
  templateUrl: "./slot-picker.html",
})
export class SlotPicker {
  readonly slots = input.required<AvailableSlot[]>();
  readonly selected = input<AvailableSlot | null>(null);
  readonly select = output<AvailableSlot>();

  protected readonly clinicTime = toClinicTime;
}
