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
  {
    path: "health-insurances",
    loadComponent: () =>
      import("./features/health-insurances/pages/health-insurance-list-page/health-insurance-list-page").then(
        (m) => m.HealthInsuranceListPage,
      ),
  },
  {
    path: "health-insurances/new",
    loadComponent: () =>
      import("./features/health-insurances/pages/health-insurance-form-page/health-insurance-form-page").then(
        (m) => m.HealthInsuranceFormPage,
      ),
  },
  {
    path: "health-insurances/:id/edit",
    loadComponent: () =>
      import("./features/health-insurances/pages/health-insurance-form-page/health-insurance-form-page").then(
        (m) => m.HealthInsuranceFormPage,
      ),
  },
  { path: "**", redirectTo: "specialties" },
];
