import { Routes } from "@angular/router";
import { authGuard, guestGuard, receptionistGuard } from "./core/auth/auth.guards";

export const routes: Routes = [
  {
    path: "login",
    canActivate: [guestGuard],
    loadComponent: () => import("./features/auth/pages/login-page/login-page").then((m) => m.LoginPage),
  },
  {
    path: "",
    canActivate: [authGuard],
    children: [
      { path: "", pathMatch: "full", redirectTo: "appointments" },
      {
        path: "appointments",
        loadComponent: () =>
          import("./features/appointments/pages/appointment-list-page/appointment-list-page").then(
            (m) => m.AppointmentListPage,
          ),
      },
      {
        path: "appointments/new",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/appointments/pages/book-appointment-page/book-appointment-page").then(
            (m) => m.BookAppointmentPage,
          ),
      },
      {
        path: "doctors",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/doctors/pages/doctor-list-page/doctor-list-page").then(
            (m) => m.DoctorListPage,
          ),
      },
      {
        path: "doctors/new",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/doctors/pages/doctor-form-page/doctor-form-page").then(
            (m) => m.DoctorFormPage,
          ),
      },
      {
        path: "doctors/:id/schedule",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/doctors/pages/doctor-schedule-page/doctor-schedule-page").then(
            (m) => m.DoctorSchedulePage,
          ),
      },
      {
        path: "doctors/:id/edit",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/doctors/pages/doctor-form-page/doctor-form-page").then(
            (m) => m.DoctorFormPage,
          ),
      },
      {
        path: "patients",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/patients/pages/patient-list-page/patient-list-page").then(
            (m) => m.PatientListPage,
          ),
      },
      {
        path: "patients/new",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/patients/pages/patient-form-page/patient-form-page").then(
            (m) => m.PatientFormPage,
          ),
      },
      {
        path: "patients/:id/edit",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/patients/pages/patient-form-page/patient-form-page").then(
            (m) => m.PatientFormPage,
          ),
      },
      {
        path: "specialties",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/specialties/pages/specialty-list-page/specialty-list-page").then(
            (m) => m.SpecialtyListPage,
          ),
      },
      {
        path: "specialties/new",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/specialties/pages/specialty-form-page/specialty-form-page").then(
            (m) => m.SpecialtyFormPage,
          ),
      },
      {
        path: "specialties/:id/edit",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/specialties/pages/specialty-form-page/specialty-form-page").then(
            (m) => m.SpecialtyFormPage,
          ),
      },
      {
        path: "health-insurances",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/health-insurances/pages/health-insurance-list-page/health-insurance-list-page").then(
            (m) => m.HealthInsuranceListPage,
          ),
      },
      {
        path: "health-insurances/new",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/health-insurances/pages/health-insurance-form-page/health-insurance-form-page").then(
            (m) => m.HealthInsuranceFormPage,
          ),
      },
      {
        path: "health-insurances/:id/edit",
        canActivate: [receptionistGuard],
        loadComponent: () =>
          import("./features/health-insurances/pages/health-insurance-form-page/health-insurance-form-page").then(
            (m) => m.HealthInsuranceFormPage,
          ),
      },
      {
        path: "users",
        canActivate: [receptionistGuard],
        loadComponent: () => import("./features/users/pages/users-page/users-page").then((m) => m.UsersPage),
      },
    ],
  },
  { path: "**", redirectTo: "appointments" },
];
