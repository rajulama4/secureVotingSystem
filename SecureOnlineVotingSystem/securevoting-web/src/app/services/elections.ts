import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface ElectionRow {
  electionId: number;
  title: string;
  description?: string | null;
  startTime: string;
  endTime: string;
  isClosed: boolean;
  isPublished: boolean;
  createdBy?: number | null;
  createdAt?: string | null;
  jurisdictionId?: number | null;
}

@Injectable({
  providedIn: 'root'
})
export class ElectionsService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAll() {
    return this.http.get<ElectionRow[]>(`${this.base}/api/elections`);
  }


   getMine() {
    return this.http.get<ElectionRow[]>(`${this.base}/api/elections/mine`);
  }

  create(payload: {
    title: string;
    description?: string;
    startTime: string;
    endTime: string;
    jurisdictionId: number;
  }) {
    return this.http.post<any>(`${this.base}/api/elections`, payload);
  }

  closeElection(id: number) {
    return this.http.post<any>(`${this.base}/api/elections/${id}/close`, {});
  }

  publishResults(id: number) {
    return this.http.post<any>(`${this.base}/api/elections/${id}/publish-results`, {});
  }

  
  getCandidatesByElection(electionId: number) {
    return this.http.get<any>(`${this.base}/api/elections/${electionId}/candidates`);
  }

   getJurisdictions() {
    return this.http.get<any[]>(`${this.base}/api/jurisdictions`);
  }

  getVoteCountStatus() {
  return this.http.get<any[]>(`${this.base}/api/elections/count-status`);
  }


  getElectionResultDetails(electionId: number) {
  return this.http.get<any[]>(`${this.base}/api/elections/${electionId}/result-details`);
}
}