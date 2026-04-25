import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule,Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { ElectionsService } from '../../services/elections';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

type VoteCountStatusRow = {
  electionId: number;
  title: string;
  jurisdictionId?: number | null;
  jurisdictionName?: string | null;
  startTime: string;
  endTime: string;
  isClosed: boolean;
  isPublished: boolean;
  totalVotes: number;
  countStatus: string;
};

type ElectionResultDetailRow = {
  candidateId: number;
  candidateName: string;
  party?: string | null;
  voteCount: number;
  isWinner: boolean;
};

@Component({
  selector: 'app-vote-count-status',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './vote-count-status.html',
  styleUrls: ['./vote-count-status.css']
})
export class VoteCountStatusComponent implements OnInit {
  rows: VoteCountStatusRow[] = [];
  loading = false;
  error = '';

  selectedElection: VoteCountStatusRow | null = null;
  resultRows: ElectionResultDetailRow[] = [];
  loadingDetails = false;
  detailsError = '';

  constructor(
    private electionsSvc: ElectionsService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadStatus();
  }

  loadStatus() {
    this.loading = true;
    this.error = '';

    this.electionsSvc.getVoteCountStatus()
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (res: any[]) => {
          this.rows = Array.isArray(res) ? res : [];
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Failed to load vote count status.';
          this.cdr.detectChanges();
        }
      });
  }

  openElectionDetails(row: VoteCountStatusRow) {
    this.router.navigate(['/admin/vote-count-status', row.electionId]);

  }

  closeDetails() {
    this.selectedElection = null;
    this.resultRows = [];
    this.detailsError = '';
    this.cdr.detectChanges();
  }

  getStatusClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'published': return 'published';
      case 'counting': return 'counting';
      case 'active': return 'active';
      case 'upcoming': return 'upcoming';
      case 'ended': return 'ended';
      default: return 'default';
    }
  }

  getResultLabel(row: ElectionResultDetailRow): string {
    if (!this.selectedElection?.isPublished) return '';
    return row.isWinner ? 'Winner' : 'Lost';
  }
}