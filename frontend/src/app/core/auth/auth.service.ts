import { HttpClient } from "@angular/common/http";
import { computed, inject, Service, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Observable, tap } from "rxjs";
import { environment } from "../../../environments/environment";
import { LoginRequest, Session } from "./auth.model";

const STORAGE_KEY = "clinic-booking.session";

/**
 * Keeps the signed-in session for the current browser tab.
 * sessionStorage (not localStorage) so closing the tab signs out; the token also expires on its own.
 */
@Service()
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly session = signal<Session | null>(readStoredSession());

  readonly currentSession = this.session.asReadonly();
  readonly isAuthenticated = computed(() => this.session() !== null);
  readonly isReceptionist = computed(() => this.session()?.role === "Receptionist");
  readonly isDoctor = computed(() => this.session()?.role === "Doctor");

  login(request: LoginRequest): Observable<Session> {
    return this.http
      .post<Session>(`${environment.apiUrl}/auth/login`, request)
      .pipe(tap((session) => this.store(session)));
  }

  /** Clears the session and goes to the login screen, remembering where the user was. */
  logout(returnUrl?: string): void {
    this.store(null);
    this.router.navigate(["/login"], { queryParams: returnUrl ? { returnUrl } : {} });
  }

  /** The token to send, or null if there is none or it already expired. */
  accessToken(): string | null {
    const session = this.session();
    if (!session) {
      return null;
    }

    if (isExpired(session)) {
      this.store(null);
      return null;
    }

    return session.accessToken;
  }

  private store(session: Session | null): void {
    this.session.set(session);
    try {
      if (session) {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
      } else {
        sessionStorage.removeItem(STORAGE_KEY);
      }
    } catch {
      // Storage can be unavailable (private mode, blocked site data); the in-memory session still works.
    }
  }
}

function isExpired(session: Session): boolean {
  return Date.parse(session.expiresAt) <= Date.now();
}

function readStoredSession(): Session | null {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    const session = raw ? (JSON.parse(raw) as Session) : null;
    return session && !isExpired(session) ? session : null;
  } catch {
    return null;
  }
}
