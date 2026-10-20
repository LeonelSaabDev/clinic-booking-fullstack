import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { LoginPage } from './login-page';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  let element: HTMLElement;
  let httpTesting: HttpTestingController;
  let navigateByUrl: ReturnType<typeof vi.spyOn>;

  beforeEach(async () => {
    sessionStorage.clear();
    await TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    navigateByUrl = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    fixture = TestBed.createComponent(LoginPage);
    element = fixture.nativeElement as HTMLElement;
    httpTesting = TestBed.inject(HttpTestingController);
    await fixture.whenStable();
  });

  afterEach(() => {
    httpTesting.verify();
    sessionStorage.clear();
  });

  function fillAndSubmit(email: string, password: string): void {
    for (const [selector, value] of [['#email', email], ['#password', password]]) {
      const input = element.querySelector<HTMLInputElement>(selector)!;
      input.value = value;
      input.dispatchEvent(new Event('input'));
    }
    element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
  }

  const session = {
    accessToken: 't',
    expiresAt: new Date(Date.now() + 3_600_000).toISOString(),
    email: 'recepcion@clinica.com',
    role: 'Receptionist',
    doctorId: null,
  };

  it('goes back to the page the user was trying to open', () => {
    fixture.componentRef.setInput('returnUrl', '/patients');

    fillAndSubmit('recepcion@clinica.com', 'secret passphrase');
    httpTesting.expectOne(`${environment.apiUrl}/auth/login`).flush(session);

    expect(navigateByUrl).toHaveBeenCalledWith('/patients');
  });

  it('ignores a return URL pointing to another site', () => {
    fixture.componentRef.setInput('returnUrl', '//evil.example.com');

    fillAndSubmit('recepcion@clinica.com', 'secret passphrase');
    httpTesting.expectOne(`${environment.apiUrl}/auth/login`).flush(session);

    expect(navigateByUrl).toHaveBeenCalledWith('/appointments');
  });

  it('shows the API message and clears the password when credentials are wrong', async () => {
    fillAndSubmit('recepcion@clinica.com', 'wrong');
    httpTesting
      .expectOne(`${environment.apiUrl}/auth/login`)
      .flush({ detail: 'Invalid email or password.' }, { status: 401, statusText: 'Unauthorized' });
    await fixture.whenStable();

    expect(element.textContent).toContain('Invalid email or password.');
    expect(element.querySelector<HTMLInputElement>('#password')!.value).toBe('');
    expect(navigateByUrl).not.toHaveBeenCalled();
  });
});
