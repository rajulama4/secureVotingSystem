import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';

import { ElectionsService } from '../../services/elections';
import { VotingService } from '../../services/voting';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

type ElectionRow = {
  electionId: number;
  title: string;
  description?: string | null;
  startTime: string;
  endTime: string;
  isClosed: boolean;
  isPublished: boolean;
  hasVoted?: boolean;
};

type CandidateRow = {
  candidateId: number;
  electionId: number;
  candidateName: string;
  party?: string | null;
  bio?: string | null;
  isActive: boolean;
};

@Component({
  selector: 'app-voter-elections',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './voter-elections.html',
  styleUrl: './voter-elections.css'
})
export class VoterElectionsComponent implements OnInit {
  elections: ElectionRow[] = [];
  selectedElection: ElectionRow | null = null;

  candidates: CandidateRow[] = [];
  selectedCandidateId: number | null = null;

  loadingElections = false;
  loadingCandidates = false;
  castingVote = false;

  error = '';
  success = '';

  constructor(
    private electionsSvc: ElectionsService,
    private votingSvc: VotingService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadElections();
  }

  loadElections(): void {
    console.log('loadElections started');

    this.loadingElections = true;
    this.error = '';
    this.success = '';

    this.electionsSvc.getMine()
      .pipe(finalize(() => {
        console.log('loadElections finalize fired');
        this.loadingElections = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (data: any) => {
          console.log('loadElections next fired');
          console.log('Raw elections response:', data);

          const list = Array.isArray(data) ? data : [];
          this.elections = list.filter((e: ElectionRow) => !e.isClosed);

          console.log('Filtered elections:', this.elections);
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          console.log('loadElections error fired');
          console.error('Load elections error:', err);

          this.error =
            err?.error?.message ||
            err?.message ||
            'Failed to load elections.';

          this.cdr.detectChanges();
        }
      });
  }

  selectElection(row: ElectionRow): void {
    if (row.hasVoted) {
      this.error = 'You already voted in this election.';
      this.cdr.detectChanges();
      return;
    }

    this.selectedElection = row;
    this.selectedCandidateId = null;
    this.candidates = [];
    this.error = '';
    this.success = '';

    this.cdr.detectChanges();
    this.loadCandidates(row.electionId);
  }

  loadCandidates(electionId: number): void {
    this.loadingCandidates = true;
    this.error = '';
    this.cdr.detectChanges();

    this.electionsSvc.getCandidatesByElection(electionId)
      .pipe(finalize(() => {
        this.loadingCandidates = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (res: any) => {
          console.log('Candidates response:', res);
          this.candidates = Array.isArray(res) ? res : [];
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          console.error('Load candidates error:', err);
          this.error = err?.error?.message ?? 'Failed to load candidates.';
          this.cdr.detectChanges();
        }
      });
  }

  castVote(): void {
  this.error = '';
  this.success = '';

  if (!this.selectedElection) {
    this.error = 'Please select an election.';
    this.cdr.detectChanges();
    return;
  }

  if (!this.selectedCandidateId) {
    this.error = 'Please select a candidate.';
    this.cdr.detectChanges();
    return;
  }

  const selectedCandidate = this.candidates.find(
    c => c.candidateId === this.selectedCandidateId
  );

  if (!selectedCandidate) {
    this.error = 'Selected candidate not found.';
    this.cdr.detectChanges();
    return;
  }

  const confirmed = window.confirm(
    `Are you sure you want to cast your vote for ${selectedCandidate.candidateName}?`
  );

  if (!confirmed) return;

  this.castingVote = true;
  this.cdr.detectChanges();

  this.votingSvc.castVote(
    this.selectedElection.electionId,
    selectedCandidate.candidateId
  )
    .pipe(finalize(() => {
      this.castingVote = false;
      this.cdr.detectChanges();
    }))
    .subscribe({
      next: (res: any) => {
        this.success = res?.message ?? 'Vote cast successfully.';

        this.elections = this.elections.map(e =>
          e.electionId === this.selectedElection!.electionId
            ? { ...e, hasVoted: true }
            : e
        );

        this.candidates = [];
        this.selectedCandidateId = null;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('Cast vote error:', err);
        this.error = err?.error?.message ?? 'Failed to cast vote.';
        this.cdr.detectChanges();
      }
    });
}
}