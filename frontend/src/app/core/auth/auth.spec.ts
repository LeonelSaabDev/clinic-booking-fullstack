import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import { environment } from '../../../environments/environment';
import { authInterceptor } from './auth.interceptor';
import { authGuard, receptionistGuard } from './auth.guards';
import { Session } from './auth.model';
import { AuthService } from './auth.service';

describe('authentication', () => {
  const api = environment.apiUrl;
  const inOneHour = () => new Date(Date.now() + 3_600_000).toISOString();
  const receptionist = (): Session => ({
    accessToken: 'reception-token',
    expiresAt: inOneHour(),
    email: 'recepcion@clinica.com',
    role: 'Receptionist',
    doctorId: null,
  });

  let auth: AuthService;
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  afterEach(() => {
    httpTesting.verify();
    sessionStorage.clear();
  });

  function signIn(session: Session): void {
    auth.login({ email: session.email, password: 'secret passphrase' }).subscribe();
    httpTesting.expectOne(`${api}/auth/login`).flush(session);
  }

  it('keeps the session after signing in', () => {
    signIn(receptionist());

    expect(auth.isAuthenticated()).toBe(true);
    expect(auth.isReceptionist()).toBe(true);
    expect(sessionStorage.getItem('clinic-booking.session')).toContain('reception-token');
  });

  it('forgets the session on logout and goes to the login screen', () => {
    signIn(receptionist());

    auth.logout('/patients');

    expect(auth.isAuthenticated()).toBe(false);
    expect(sessionStorage.length).toBe(0);
    expect(router.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/patients' } });
  });

  it('treats an expired session as signed out', () => {
    signIn({ ...receptionist(), expiresAt: new Date(Date.now() - 1000).toISOString() });

    expect(auth.accessToken()).toBeNull();
    expect(auth.isAuthenticated()).toBe(false);
  });

  it('sends the bearer token to our API', () => {
    signIn(receptionist());

    http.get(`${api}/doctors`).subscribe();

    expect(httpTesting.expectOne(`${api}/doctors`).request.headers.get('Authorization')).toBe('Bearer reception-token');
  });

  it('never sends the token to other sites', () => {
    signIn(receptionist());

    http.get('https://example.com/data').subscribe();

    expect(httpTesting.expectOne('https://example.com/data').request.headers.has('Authorization')).toBe(false);
  });

  it('signs out when the API rejects the token', () => {
    signIn(receptionist());

    http.get(`${api}/doctors`).subscribe({ error: () => undefined });
    httpTesting.expectOne(`${api}/doctors`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(auth.isAuthenticated()).toBe(false);
  });

  it('sends anonymous visitors to the login screen, remembering where they were going', () => {
    const result = TestBed.runInInjectionContext(() => authGuard(null!, { url: '/patients' } as never));

    expect(router.serializeUrl(result as UrlTree)).toBe('/login?returnUrl=%2Fpatients');
  });

  it('sends doctors back to their agenda from receptionist-only screens', () => {
    signIn({ ...receptionist(), role: 'Doctor', doctorId: 'doc-1' });

    const result = TestBed.runInInjectionContext(() => receptionistGuard(null!, null!));

    expect(router.serializeUrl(result as UrlTree)).toBe('/appointments');
  });
});
