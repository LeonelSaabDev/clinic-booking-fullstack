import { HttpErrorResponse } from "@angular/common/http";
import { Component, inject, input, signal } from "@angular/core";
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { AuthService } from "../../../../core/auth/auth.service";

@Component({
  imports: [ReactiveFormsModule],
  selector: "app-login-page",
  styleUrl: "./login-page.css",
  templateUrl: "./login-page.html",
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(NonNullableFormBuilder);

  /** Bound from `?returnUrl=`, set by the guard or the interceptor. */
  readonly returnUrl = input<string>();

  protected readonly form = this.fb.group({
    email: ["", [Validators.required, Validators.email]],
    password: ["", Validators.required],
  });

  protected readonly signingIn = signal(false);
  protected readonly error = signal<string | null>(null);

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.signingIn.set(true);
    this.error.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigateByUrl(this.safeReturnUrl()),
      error: (error: HttpErrorResponse) => {
        this.signingIn.set(false);
        this.form.controls.password.reset();
        this.error.set(
          error.status === 401
            ? (error.error?.detail ?? "Invalid email or password.")
            : "The server could not be reached. Please try again.",
        );
      },
    });
  }

  /** Only follow app-internal paths, so a crafted link can't send users to another site after signing in. */
  private safeReturnUrl(): string {
    const url = this.returnUrl();
    return url && url.startsWith("/") && !url.startsWith("//") ? url : "/appointments";
  }
}
