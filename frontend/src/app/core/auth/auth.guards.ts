import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "./auth.service";

/**
 * Guards only shape navigation for a good experience. The real protection is the API,
 * which checks the signed token on every request.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  return auth.accessToken() !== null
    ? true
    : inject(Router).createUrlTree(["/login"], { queryParams: { returnUrl: state.url } });
};

export const receptionistGuard: CanActivateFn = () =>
  inject(AuthService).isReceptionist() ? true : inject(Router).createUrlTree(["/appointments"]);

/** Keeps signed-in users away from the login screen. */
export const guestGuard: CanActivateFn = () =>
  inject(AuthService).accessToken() === null ? true : inject(Router).createUrlTree(["/appointments"]);
