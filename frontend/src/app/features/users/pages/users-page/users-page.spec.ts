import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { environment } from '../../../../../environments/environment';
import { UsersPage } from './users-page';

describe('UsersPage', () => {
  const api = environment.apiUrl;
  let fixture: ComponentFixture<UsersPage>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UsersPage],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(UsersPage);
    httpTesting = TestBed.inject(HttpTestingController);
    TestBed.tick();
    httpTesting.expectOne(`${api}/users`).flush([
      { id: 'u-1', email: 'ana@clinica.com', role: 'Doctor', doctorId: 'doc-1' },
    ]);
    httpTesting.expectOne(`${api}/doctors`).flush([
      { id: 'doc-1', firstName: 'Ana', lastName: 'García', licenseNumber: 'MN 1', specialtyId: 's', healthInsuranceIds: [] },
      { id: 'doc-2', firstName: 'Luis', lastName: 'Rossi', licenseNumber: 'MN 2', specialtyId: 's', healthInsuranceIds: [] },
    ]);
    await fixture.whenStable();
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('lists accounts and only offers doctors without one', () => {
    const element = fixture.nativeElement as HTMLElement;
    const doctorOptions = [...element.querySelectorAll('#doctorId option')].map((o) => o.textContent?.trim());

    expect(element.textContent).toContain('ana@clinica.com');
    expect(doctorOptions).toContain('Rossi, Luis');
    expect(doctorOptions).not.toContain('García, Ana');
  });
});
