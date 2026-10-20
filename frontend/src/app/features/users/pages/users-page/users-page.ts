import { HttpErrorResponse } from "@angular/common/http";
import { Component, computed, inject, signal, viewChild } from "@angular/core";
import { rxResource } from "@angular/core/rxjs-interop";
import { DoctorsApi } from "../../../doctors/data-access/doctors-api";
import { UserForm } from "../../components/user-form/user-form";
import { UserTable } from "../../components/user-table/user-table";
import { CreateUserRequest, doctorsWithoutAccount, toUserRows } from "../../data-access/user.model";
import { UsersApi } from "../../data-access/users-api";

@Component({
  imports: [UserForm, UserTable],
  selector: "app-users-page",
  styleUrl: "./users-page.css",
  templateUrl: "./users-page.html",
})
export class UsersPage {
  private readonly usersApi = inject(UsersApi);
  private readonly doctorsApi = inject(DoctorsApi);

  private readonly userForm = viewChild(UserForm);

  private readonly users = rxResource({ stream: () => this.usersApi.getAll() });
  private readonly doctors = rxResource({ stream: () => this.doctorsApi.getAll() });

  protected readonly isLoading = computed(() => this.users.isLoading() || this.doctors.isLoading());
  protected readonly hasError = computed(() => !!(this.users.error() || this.doctors.error()));
  protected readonly rows = computed(() => toUserRows(this.users.value() ?? [], this.doctors.value() ?? []));
  protected readonly availableDoctors = computed(() =>
    doctorsWithoutAccount(this.doctors.value() ?? [], this.users.value() ?? []),
  );

  protected readonly saving = signal(false);
  protected readonly serverError = signal<string | null>(null);
  protected readonly createdEmail = signal<string | null>(null);

  protected create(request: CreateUserRequest): void {
    this.saving.set(true);
    this.serverError.set(null);
    this.createdEmail.set(null);

    this.usersApi.create(request).subscribe({
      next: (user) => {
        this.saving.set(false);
        this.createdEmail.set(user.email);
        this.userForm()?.reset();
        this.users.reload();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.serverError.set(error.error?.detail ?? "The account could not be created.");
      },
    });
  }
}
