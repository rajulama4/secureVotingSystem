import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs';

import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';

import {
  ReportsService,
  AdminReportsOverview,
  VoterVerificationStatusReport,
  ElectionTurnoutReport,
  CandidatePerformanceReport
} from '../../services/reports.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatButtonModule
  ],
  templateUrl: './reports.html',
  styleUrl: './reports.css'
})
export class ReportsComponent implements OnInit {
  loading = false;
  error = '';

  overview?: AdminReportsOverview;

  verificationStatus: VoterVerificationStatusReport[] = [];
  electionTurnout: ElectionTurnoutReport[] = [];
  candidatePerformance: CandidatePerformanceReport[] = [];

  verificationColumns: string[] = ['verificationStatus', 'totalCount'];

  turnoutColumns: string[] = [
    'electionTitle',
    'totalEligibleVoters',
    'totalVotesCast',
    'turnoutPercentage'
  ];

  candidateColumns: string[] = [
    'electionTitle',
    'candidateName',
    'party',
    'voteCount',
    'votePercentage'
  ];

  constructor(
    private reportsService: ReportsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    this.error = '';

    let completed = 0;
    const totalRequests = 4;

    const finish = () => {
      completed++;

      if (completed === totalRequests) {
        this.loading = false;
        this.cdr.detectChanges();
      }
    };

    this.reportsService
      .getOverview()
      .pipe(finalize(() => finish()))
      .subscribe({
        next: (res) => {
          this.overview = res;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Overview report error:', err);
          this.error = 'Failed to load overview report.';
          this.cdr.detectChanges();
        }
      });

    this.reportsService
      .getVotersByVerificationStatus()
      .pipe(finalize(() => finish()))
      .subscribe({
        next: (res) => {
          this.verificationStatus = res;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Verification report error:', err);
          this.error = 'Failed to load verification status report.';
          this.cdr.detectChanges();
        }
      });

    this.reportsService
      .getElectionTurnout()
      .pipe(finalize(() => finish()))
      .subscribe({
        next: (res) => {
          this.electionTurnout = res;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Election turnout report error:', err);
          this.error = 'Failed to load election turnout report.';
          this.cdr.detectChanges();
        }
      });

    this.reportsService
      .getCandidatePerformance()
      .pipe(finalize(() => finish()))
      .subscribe({
        next: (res) => {
          this.candidatePerformance = res;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Candidate performance report error:', err);
          this.error = 'Failed to load candidate performance report.';
          this.cdr.detectChanges();
        }
      });
  }

  refresh(): void {
    this.loadReports();
  }
}