import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CandidatesService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  create(formData: FormData) {
    return this.http.post<any>(`${this.base}/api/candidates`, formData);
  }

  getByElection(electionId: number) {
    return this.http.get<any[]>(`${this.base}/api/candidates/election/${electionId}`);
  }

  deactivate(candidateId: number) {
    return this.http.post<any>(`${this.base}/api/candidates/${candidateId}/deactivate`, {});
  }

  update(formData: FormData) {
  return this.http.post<any>(`${this.base}/api/candidates/update`, formData);
}

  
}