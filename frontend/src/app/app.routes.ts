import { Routes } from "@angular/router";

export const routes: Routes = [
  { path: "", pathMatch: "full", redirectTo: "specialties" },
  {
    path: "specialties",
    loadComponent: () =>
      import("./features/specialties/pages/specialty-list-page/specialty-list-page").then(
        (m) => m.SpecialtyListPage,
      ),
  },
  {
    path: "specialties/new",
    loadComponent: () =>
      import("./features/specialties/pages/specialty-form-page/specialty-form-page").then(
        (m) => m.SpecialtyFormPage,
      ),
  },
  {
    path: "specialties/:id/edit",
    loadComponent: () =>
      import("./features/specialties/pages/specialty-form-page/specialty-form-page").then(
        (m) => m.SpecialtyFormPage,
      ),
  },
  { path: "**", redirectTo: "specialties" },
];
