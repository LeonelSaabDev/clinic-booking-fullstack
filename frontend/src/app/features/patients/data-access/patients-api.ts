import { HttpClient, HttpParams } from "@angular/common/http";
import { inject, Service } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../../../environments/environment";
import { Patient, PatientRequest } from "./patient.model";

@Service()
export class PatientsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/patients`;

  search(term = ""): Observable<Patient[]> {
    const params = term.trim() ? new HttpParams().set("search", term.trim()) : undefined;
    return this.http.get<Patient[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Patient> {
    return this.http.get<Patient>(`${this.baseUrl}/${id}`);
  }

  create(request: PatientRequest): Observable<Patient> {
    return this.http.post<Patient>(this.baseUrl, request);
  }

  update(id: string, request: PatientRequest): Observable<Patient> {
    return this.http.put<Patient>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
