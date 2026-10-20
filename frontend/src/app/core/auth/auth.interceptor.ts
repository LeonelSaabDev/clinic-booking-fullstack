import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, throwError } from "rxjs";
import { environment } from "../../../environments/environment";
import { AuthService } from "./auth.service";

/**
 * Adds the bearer token to calls to our own API only (never to third-party URLs),
 * and signs out when the API says the session is no longer valid.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith(environment.apiUrl)) {
    return next(request);
  }

  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.accessToken();
  const authorized = token ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : request;

  return next(authorized).pipe(
    catchError((error: unknown) => {
      const isLoginCall = request.url.endsWith("/auth/login");
      if (error instanceof HttpErrorResponse && error.status === 401 && !isLoginCall) {
        auth.logout(router.url);
      }
      return throwError(() => error);
    }),
  );
};
