import { HttpErrorResponse } from "@angular/common/http";
import { Component, inject } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { SpecialtyTable } from "../../components/specialty-table/specialty-table";
import { Specialty } from "../../data-access/specialty.model";
import { SpecialtiesApi } from "../../data-access/specialties-api";

@Component({
  imports: [SpecialtyTable, RouterLink],
  selector: "app-specialty-list-page",
  templateUrl: "./specialty-list-page.html",
})
export class SpecialtyListPage {
  private readonly api = inject(SpecialtiesApi);
  private readonly router = inject(Router);

  protected readonly specialties = rxResource({
    stream: () => this.api.getAll(),
  });

  protected renameSpecialty(specialty: Specialty): void {
    this.router.navigate(["/specialties", specialty.id, "edit"]);
  }

  protected deleteSpecialty(specialty: Specialty): void {
    if (!confirm(`Delete "${specialty.name}"?`)) {
      return;
    }

    this.api.delete(specialty.id).subscribe({
      next: () => this.specialties.reload(),
      error: (error: HttpErrorResponse) =>
        alert(error.error?.detail ?? "The specialty could not be deleted. Please try again."),
    });
  }
}
