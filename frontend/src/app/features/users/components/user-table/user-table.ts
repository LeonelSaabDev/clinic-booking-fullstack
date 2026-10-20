import { Component, input } from "@angular/core";
import { UserRow } from "../../data-access/user.model";

@Component({
  selector: "app-user-table",
  styleUrl: "./user-table.css",
  templateUrl: "./user-table.html",
})
export class UserTable {
  readonly users = input.required<UserRow[]>();
}
