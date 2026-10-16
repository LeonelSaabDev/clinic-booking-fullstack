import { HttpClient } from "@angular/common/http";
import { inject, Service } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../../../environments/environment";
import { Specialty, SpecialtyRequest } from "./specialty.model";

@Service()
export class SpecialtiesApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/specialties`;

  getAll(): Observable<Specialty[]> {
    return this.http.get<Specialty[]>(this.baseUrl);
  }

  getById(id: string): Observable<Specialty> {
    return this.http.get<Specialty>(`${this.baseUrl}/${id}`);
  }

  create(request: SpecialtyRequest): Observable<Specialty> {
    return this.http.post<Specialty>(this.baseUrl, request);
  }

  rename(id: string, request: SpecialtyRequest): Observable<Specialty> {
    return this.http.put<Specialty>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
