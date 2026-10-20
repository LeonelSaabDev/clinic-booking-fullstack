import { HttpClient, HttpParams } from "@angular/common/http";
import { inject, Service } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../../../environments/environment";
import { AvailableSlot, DoctorSchedule } from "./doctor-schedule.model";
import { Doctor, DoctorRequest } from "./doctor.model";

@Service()
export class DoctorsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/doctors`;

  getAll(): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(this.baseUrl);
  }

  getById(id: string): Observable<Doctor> {
    return this.http.get<Doctor>(`${this.baseUrl}/${id}`);
  }

  create(request: DoctorRequest): Observable<Doctor> {
    return this.http.post<Doctor>(this.baseUrl, request);
  }

  update(id: string, request: DoctorRequest): Observable<Doctor> {
    return this.http.put<Doctor>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getSchedule(id: string): Observable<DoctorSchedule> {
    return this.http.get<DoctorSchedule>(`${this.baseUrl}/${id}/schedule`);
  }

  updateSchedule(id: string, schedule: DoctorSchedule): Observable<DoctorSchedule> {
    return this.http.put<DoctorSchedule>(`${this.baseUrl}/${id}/schedule`, schedule);
  }

  /** @param date "yyyy-MM-dd" */
  getAvailability(id: string, date: string): Observable<AvailableSlot[]> {
    const params = new HttpParams().set("date", date);
    return this.http.get<AvailableSlot[]>(`${this.baseUrl}/${id}/availability`, { params });
  }
}
