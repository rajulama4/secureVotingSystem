import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { ElectionsService } from '../../services/elections';
import { environment } from '../../../environments/environment';

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
  candidateImagePath?: string | null;
  voteCount: number;
  isWinner: boolean;
};

@Component({
  selector: 'app-vote-count-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './vote-count-detail.html',
  styleUrls: ['./vote-count-detail.css']
})
export class VoteCountDetailComponent implements OnInit {
  electionId = 0;
  election: VoteCountStatusRow | null = null;
  resultRows: ElectionResultDetailRow[] = [];

  loading = false;
  error = '';

  apiBaseUrl = environment.apiUrl;

  constructor(
    private route: ActivatedRoute,
    private electionsSvc: ElectionsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.electionId = Number(this.route.snapshot.paramMap.get('electionId'));
    this.loadPage();
  }

  loadPage() {
    if (!this.electionId) {
      this.error = 'Invalid election id.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.electionsSvc.getVoteCountStatus()
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (rows: any[]) => {
          const all = Array.isArray(rows) ? rows : [];
          this.election = all.find(x => x.electionId === this.electionId) || null;

          if (!this.election) {
            this.error = 'Election not found.';
            this.cdr.detectChanges();
            return;
          }

          this.loadDetails();
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Failed to load election summary.';
          this.cdr.detectChanges();
        }
      });
  }

  loadDetails() {
    this.loading = true;
    this.cdr.detectChanges();

    this.electionsSvc.getElectionResultDetails(this.electionId)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (res: any[]) => {
          this.resultRows = Array.isArray(res) ? res : [];
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.error = err?.error?.message ?? 'Failed to load election results.';
          this.cdr.detectChanges();
        }
      });
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
    if (!this.election?.isPublished) return '';
    return row.isWinner ? 'Winner' : 'Lost';
  }

  getMaxVotes(): number {
    if (!this.resultRows.length) return 0;
    return Math.max(...this.resultRows.map(x => x.voteCount), 0);
  }

  getBarWidth(voteCount: number): number {
    const max = this.getMaxVotes();
    if (max <= 0) return 0;
    return (voteCount / max) * 100;
  }

  getVotePercent(voteCount: number): number {
    const total = this.election?.totalVotes ?? 0;
    if (total <= 0) return 0;
    return Math.round((voteCount / total) * 100);
  }

  getWinner(): ElectionResultDetailRow | null {
    return this.resultRows.find(x => x.isWinner) || null;
  }

  getImageUrl(path?: string | null): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return this.apiBaseUrl + path;
  }
}