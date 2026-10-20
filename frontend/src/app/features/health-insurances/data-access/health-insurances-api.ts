import { HttpClient } from "@angular/common/http";
import { inject, Service } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../../../environments/environment";
import {
  HealthInsurance,
  HealthInsuranceRequest,
} from "./health-insurance.model";

@Service()
export class HealthInsurancesApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/health-insurances`;

  getAll(): Observable<HealthInsurance[]> {
    return this.http.get<HealthInsurance[]>(this.baseUrl);
  }

  getById(id: string): Observable<HealthInsurance> {
    return this.http.get<HealthInsurance>(`${this.baseUrl}/${id}`);
  }

  create(request: HealthInsuranceRequest): Observable<HealthInsurance> {
    return this.http.post<HealthInsurance>(this.baseUrl, request);
  }

  rename(
    id: string,
    request: HealthInsuranceRequest,
  ): Observable<HealthInsurance> {
    return this.http.put<HealthInsurance>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
