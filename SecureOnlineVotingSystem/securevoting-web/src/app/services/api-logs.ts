import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface ApiLogRow {
  logId: number;
  userId?: number | null;
  email?: string | null;
  endpoint: string;
  httpMethod: string;
  requestTimeUtc: string;
  ipAddress?: string | null;
  userAgent?: string | null;
  isSuccess: boolean;
  statusCode: number;
  durationMs?: number | null;
}

export interface ApiLogDetail extends ApiLogRow {
  apiReq?: string | null;
  apiRes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ApiLogsService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  list(filters: { take?: number; skip?: number; email?: string; userId?: number; endpoint?: string }) {
    let params = new HttpParams()
      .set('take', String(filters.take ?? 50))
      .set('skip', String(filters.skip ?? 0));

    if (filters.email) params = params.set('email', filters.email);
    if (filters.endpoint) params = params.set('endpoint', filters.endpoint);
    if (filters.userId != null) params = params.set('userId', String(filters.userId));

    return this.http.get<ApiLogRow[]>(`${this.base}/api/admin/api-logs`, { params });
  }

  detail(logId: number) {
    return this.http.get<ApiLogDetail>(`${this.base}/api/admin/api-logs/${logId}`);
  }
}