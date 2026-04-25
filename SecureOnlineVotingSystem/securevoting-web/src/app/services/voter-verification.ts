import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface VoterVerificationPayload {
  legalFullName: string;
  dateOfBirth?: string | null;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  stateCode: string;
  zipCode: string;
  jurisdictionId?: number | null;
  idDocumentType: string;
  idDocumentNumberMasked?: string | null;
  idDocumentState?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class VoterVerificationService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  submit(payload: VoterVerificationPayload) {
    return this.http.post<any>(`${this.base}/api/voter-verification/submit`, payload);
  }

  getMine() {
    return this.http.get<any>(`${this.base}/api/voter-verification/me`);
  }

  getPending() {
    return this.http.get<any[]>(`${this.base}/api/voter-verification/pending`);
  }

  getAll() {
    return this.http.get<any[]>(`${this.base}/api/voter-verification/all`);
  }

  getJurisdictions() {
    return this.http.get<any[]>(`${this.base}/api/jurisdictions`);
  }

  approve(userId: number, reviewerNotes: string, jurisdictionId: number) {
    return this.http.post<any>(`${this.base}/api/voter-verification/${userId}/approve`, {
      reviewerNotes: reviewerNotes || null,
      jurisdictionId
    });
  }

  reject(userId: number, reviewerNotes: string) {
    return this.http.post<any>(`${this.base}/api/voter-verification/${userId}/reject`, {
      reviewerNotes
    });
  }
}