import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { ElectionsService } from '../../services/elections';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

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

@Component({
  selector: 'app-vote-count-status',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './vote-count-status.html',
  styleUrls: ['./vote-count-status.css']
})
export class VoteCountStatusComponent implements OnInit {
  rows: VoteCountStatusRow[] = [];
  loading = false;
  error = '';

  constructor(
    private electionsSvc: ElectionsService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadStatus();
  }

  loadStatus(): void {
    this.loading = true;
    this.error = '';

    this.electionsSvc.getVoteCountStatus()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
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

  openElectionDetails(row: VoteCountStatusRow): void {
    this.router.navigate(['/admin/vote-count-status', row.electionId]);
  }

  getStatusClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'published':
        return 'published';
      case 'counting':
        return 'counting';
      case 'active':
        return 'active';
      case 'upcoming':
        return 'upcoming';
      case 'ended':
        return 'ended';
      case 'closed':
        return 'ended';
      default:
        return 'default';
    }
  }

  getPublishedCount(): number {
    return this.rows.filter(x => x.isPublished).length;
  }

  getEndedCount(): number {
    return this.rows.filter(x =>
      ['ended', 'closed'].includes((x.countStatus || '').toLowerCase())
    ).length;
  }

  getActiveCount(): number {
    return this.rows.filter(x =>
      (x.countStatus || '').toLowerCase() === 'active'
    ).length;
  }

  getTotalVotes(): number {
    return this.rows.reduce((sum, x) => sum + (x.totalVotes || 0), 0);
  }
}