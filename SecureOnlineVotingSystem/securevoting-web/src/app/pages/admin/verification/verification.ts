import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { RouterModule } from '@angular/router';

import { VoterVerificationService } from '../../../services/voter-verification';
import { environment } from '../../../../environments/environment';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';

type VerificationRow = {
  verificationId: number;
  userId: number;
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
  isIdentityVerified: boolean;
  isResidenceVerified: boolean;
  isEligibleToVote: boolean;
  verificationStatus: string;
  reviewerNotes?: string | null;
  reviewedByUserId?: number | null;
  reviewedAtUtc?: string | null;
  submittedAtUtc: string;
  updatedAtUtc: string;
  idPicturePath?: string | null;
};

type JurisdictionRow = {
  jurisdictionId: number;
  jurisdictionName: string;
  county?: string | null;
  city?: string | null;
  zipCode?: string | null;
};

@Component({
  selector: 'app-verification',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule
  ],
  templateUrl: './verification.html',
  styleUrl: './verification.css'
})
export class VerificationComponent {
  rows: VerificationRow[] = [];
  jurisdictions: JurisdictionRow[] = [];

  loading = false;
  error = '';
  success = '';

  notesByUserId: { [key: number]: string } = {};
  jurisdictionByUserId: { [key: number]: number | null } = {};

  apiBaseUrl = environment.apiUrl;

  constructor(
    private verificationSvc: VoterVerificationService,
    private cdr: ChangeDetectorRef
  ) {
    this.loadJurisdictions();
    this.loadPending();
  }

  loadPending(): void {
    this.loading = true;
    this.error = '';
    this.success = '';

    this.verificationSvc.getPending()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (data: any) => {
          this.rows = Array.isArray(data) ? data : [];

          for (const row of this.rows) {
            this.jurisdictionByUserId[row.userId] = row.jurisdictionId ?? null;
          }

          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('PENDING ERROR:', err);
          this.error = err?.error?.message ?? 'Failed to load pending verifications.';
          this.cdr.detectChanges();
        }
      });
  }

  loadJurisdictions(): void {
    this.verificationSvc.getJurisdictions().subscribe({
      next: (data: any) => {
        this.jurisdictions = Array.isArray(data) ? data : [];
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('JURISDICTIONS ERROR:', err);
        this.error = err?.error?.message ?? 'Failed to load jurisdictions.';
        this.cdr.detectChanges();
      }
    });
  }

  approve(row: VerificationRow): void {
    this.error = '';
    this.success = '';

    const jurisdictionId = this.jurisdictionByUserId[row.userId];

    if (!jurisdictionId) {
      this.error = 'Please select jurisdiction before approving.';
      this.cdr.detectChanges();
      return;
    }

    const notes = this.notesByUserId[row.userId]?.trim() || '';

    this.verificationSvc.approve(row.userId, notes, jurisdictionId).subscribe({
      next: (res: any) => {
        this.success = res?.message ?? 'Voter verification approved.';
        delete this.notesByUserId[row.userId];
        delete this.jurisdictionByUserId[row.userId];
        this.loadPending();
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Approval failed.';
        this.cdr.detectChanges();
      }
    });
  }

  reject(row: VerificationRow): void {
    this.error = '';
    this.success = '';

    const notes = this.notesByUserId[row.userId]?.trim() || '';

    if (!notes) {
      this.error = 'Reviewer notes are required for rejection.';
      this.cdr.detectChanges();
      return;
    }

    this.verificationSvc.reject(row.userId, notes).subscribe({
      next: (res: any) => {
        this.success = res?.message ?? 'Voter verification rejected.';
        delete this.notesByUserId[row.userId];
        delete this.jurisdictionByUserId[row.userId];
        this.loadPending();
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to reject voter verification.';
        this.cdr.detectChanges();
      }
    });
  }

  getFullImageUrl(path: string | null | undefined): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return this.apiBaseUrl + path;
  }
}