import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export interface OpenElection {
  electionId: number;
  title: string;
  description?: string | null;
  startTime: string;
  endTime: string;
  isClosed: boolean;
  isPublished: boolean;
}

export interface Candidate {
  candidateId: number;
  electionId: number;
  candidateName: string;
  party?: string | null;
  bio?: string | null;
}

export interface VoteResult {
  candidateId: number;
  candidateName: string;
  party?: string | null;
  voteCount: number;
}

@Injectable({ providedIn: 'root' })
export class VotingService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getOpenElections() {
    return this.http.get<OpenElection[]>(`${this.base}/api/vote/open-elections`);
  }

  getCandidates(electionId: number) {
    return this.http.get<Candidate[]>(`${this.base}/api/vote/elections/${electionId}/candidates`);
  }

castVote(electionId: number, candidateId: number) {
  return this.http.post<any>(`${this.base}/api/vote/cast`, {
    electionId,
    candidateId
  });
}

  getResults(electionId: number) {
    return this.http.get<VoteResult[]>(`${this.base}/api/vote/results/${electionId}`);
  }

  
}