import { Component, inject } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { SpecialtiesApi } from "../../data-access/specialties-api";

@Component({
  selector: "app-specialty-list-page",
  templateUrl: "./specialty-list-page.html",
})
export class SpecialtyListPage {
  private readonly api = inject(SpecialtiesApi);

  protected readonly specialties = rxResource({
    stream: () => this.api.getAll(),
  });
}
