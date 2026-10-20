import { Routes } from "@angular/router";

export const routes: Routes = [
  { path: "", pathMatch: "full", redirectTo: "doctors" },
  {
    path: "doctors",
    loadComponent: () =>
      import("./features/doctors/pages/doctor-list-page/doctor-list-page").then(
        (m) => m.DoctorListPage,
      ),
  },
  {
    path: "doctors/new",
    loadComponent: () =>
      import("./features/doctors/pages/doctor-form-page/doctor-form-page").then(
        (m) => m.DoctorFormPage,
      ),
  },
  {
    path: "doctors/:id/schedule",
    loadComponent: () =>
      import("./features/doctors/pages/doctor-schedule-page/doctor-schedule-page").then(
        (m) => m.DoctorSchedulePage,
      ),
  },
  {
    path: "doctors/:id/edit",
    loadComponent: () =>
      import("./features/doctors/pages/doctor-form-page/doctor-form-page").then(
        (m) => m.DoctorFormPage,
      ),
  },
  {
    path: "patients",
    loadComponent: () =>
      import("./features/patients/pages/patient-list-page/patient-list-page").then(
        (m) => m.PatientListPage,
      ),
  },
  {
    path: "patients/new",
    loadComponent: () =>
      import("./features/patients/pages/patient-form-page/patient-form-page").then(
        (m) => m.PatientFormPage,
      ),
  },
  {
    path: "patients/:id/edit",
    loadComponent: () =>
      import("./features/patients/pages/patient-form-page/patient-form-page").then(
        (m) => m.PatientFormPage,
      ),
  },
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
  { path: "**", redirectTo: "doctors" },
];
