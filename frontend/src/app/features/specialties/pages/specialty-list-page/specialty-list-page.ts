import { Component, inject } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { SpecialtyTable } from "../../components/specialty-table/specialty-table";
import { Specialty } from "../../data-access/specialty.model";
import { SpecialtiesApi } from "../../data-access/specialties-api";

@Component({
  imports: [SpecialtyTable],
  selector: "app-specialty-list-page",
  templateUrl: "./specialty-list-page.html",
})
export class SpecialtyListPage {
  private readonly api = inject(SpecialtiesApi);

  protected readonly specialties = rxResource({
    stream: () => this.api.getAll(),
  });

  protected deleteSpecialty(specialty: Specialty): void {
    if (!confirm(`Delete "${specialty.name}"?`)) {
      return;
    }

    this.api.delete(specialty.id).subscribe({
      next: () => this.specialties.reload(),
      error: () =>
        alert("The specialty could not be deleted. Please try again."),
    });
  }
}
