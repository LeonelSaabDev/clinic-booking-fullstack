import { HttpClient, HttpParams } from "@angular/common/http";
import { inject, Service } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../../../environments/environment";
import { Appointment, BookAppointmentRequest } from "./appointment.model";

@Service()
export class AppointmentsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/appointments`;

  /** @param date "yyyy-MM-dd", a clinic calendar day */
  list(date: string, doctorId?: string): Observable<Appointment[]> {
    let params = new HttpParams().set("date", date);
    if (doctorId) {
      params = params.set("doctorId", doctorId);
    }
    return this.http.get<Appointment[]>(this.baseUrl, { params });
  }

  book(request: BookAppointmentRequest): Observable<Appointment> {
    return this.http.post<Appointment>(this.baseUrl, request);
  }

  cancel(id: string): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.baseUrl}/${id}/cancel`, null);
  }

  complete(id: string): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.baseUrl}/${id}/complete`, null);
  }

  markNoShow(id: string): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.baseUrl}/${id}/no-show`, null);
  }
}
