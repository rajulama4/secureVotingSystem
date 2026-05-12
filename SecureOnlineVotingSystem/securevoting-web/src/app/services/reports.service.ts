import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface AdminReportsOverview {
  totalVoters: number;
  verifiedVoters: number;
  pendingVerifications: number;
  rejectedVerifications: number;
  totalElections: number;
  publishedElections: number;
  closedElections: number;
  totalVotesCast: number;
  totalCandidates: number;
}

export interface VoterVerificationStatusReport {
  verificationStatus: string;
  totalCount: number;
}

export interface ElectionTurnoutReport {
  electionId: number;
  electionTitle: string;
  totalEligibleVoters: number;
  totalVotesCast: number;
  turnoutPercentage: number;
}

export interface CandidatePerformanceReport {
  electionId: number;
  electionTitle: string;
  candidateId: number;
  candidateName: string;
  party?: string;
  voteCount: number;
  votePercentage: number;
}

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getOverview() {
    return this.http.get<AdminReportsOverview>(
      `${this.base}/api/reports/overview`
    );
  }

  getVotersByVerificationStatus() {
    return this.http.get<VoterVerificationStatusReport[]>(
      `${this.base}/api/reports/voters-by-verification-status`
    );
  }

  getElectionTurnout() {
    return this.http.get<ElectionTurnoutReport[]>(
      `${this.base}/api/reports/election-turnout`
    );
  }

  getCandidatePerformance() {
    return this.http.get<CandidatePerformanceReport[]>(
      `${this.base}/api/reports/candidate-performance`
    );
  }
}