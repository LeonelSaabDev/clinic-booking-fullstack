import { HttpErrorResponse } from "@angular/common/http";
import { Component, computed, inject, input, signal } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { Router, RouterLink } from "@angular/router";
import { SpecialtyForm } from "../../components/specialty-form/specialty-form";
import { SpecialtyRequest } from "../../data-access/specialty.model";
import { SpecialtiesApi } from "../../data-access/specialties-api";

@Component({
  imports: [SpecialtyForm, RouterLink],
  selector: "app-specialty-form-page",
  styleUrl: "./specialty-form-page.css",
  templateUrl: "./specialty-form-page.html",
})
export class SpecialtyFormPage {
  private readonly api = inject(SpecialtiesApi);
  private readonly router = inject(Router);

  /** Bound from the `:id` route parameter. Undefined when creating a specialty. */
  readonly id = input<string>();

  protected readonly isEditMode = computed(() => this.id() !== undefined);

  protected readonly specialty = rxResource({
    params: () => this.id(),
    stream: ({ params: id }) => this.api.getById(id),
  });

  protected readonly saving = signal(false);
  protected readonly serverError = signal<string | null>(null);

  protected save(request: SpecialtyRequest): void {
    this.saving.set(true);
    this.serverError.set(null);

    const id = this.id();
    const request$ = id
      ? this.api.rename(id, request)
      : this.api.create(request);

    request$.subscribe({
      next: () => this.router.navigate(["/specialties"]),
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.serverError.set(
          error.error?.detail ?? "The specialty could not be saved.",
        );
      },
    });
  }

  protected cancel(): void {
    this.router.navigate(["/specialties"]);
  }
}
