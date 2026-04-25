import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { VoterVerificationService } from '../../services/voter-verification';
import { environment } from '../../../environments/environment';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-voter',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './voter.html',
  styleUrl: './voter.css'
})
export class VoterComponent implements OnInit {
  verificationLoading = false;
  verificationSaving = false;

  error = '';
  success = '';

  verificationStatus = 'Not Submitted';
  verificationRecord: any = null;

  apiBaseUrl = environment.apiUrl;
  idPicturePath = '';

  legalFullName = '';
  dateOfBirth = '';
  addressLine1 = '';
  addressLine2 = '';
  city = '';
  stateCode = '';
  zipCode = '';
  phoneNumber = '';
  jurisdictionId: number | null = null;
  idDocumentType = '';
  idDocumentNumberMasked = '';
  idDocumentState = '';
  userEmail = '';
  userId = '';
  age: number | null = null;

  constructor(
    private verificationSvc: VoterVerificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadMyVerification();
  }

  loadMyVerification(): void {
    console.log('loadMyVerification started');

    this.verificationLoading = true;
    this.error = '';
    this.success = '';
    this.cdr.detectChanges();

    this.verificationSvc.getMine()
      .pipe(finalize(() => {
        console.log('loadMyVerification finalize fired');
        this.verificationLoading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (data: any) => {
          console.log('loadMyVerification next fired', data);

          this.verificationRecord = data;
          this.verificationStatus = data?.verificationStatus ?? 'Submitted';
          this.prefillVerificationForm(data);
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          console.log('loadMyVerification error fired', err);

          this.verificationRecord = null;
          this.verificationStatus = 'Not Submitted';
          this.error =
            err?.error?.message ||
            err?.message ||
            'Failed to load verification status.';

          this.cdr.detectChanges();
        }
      });
  }

  prefillVerificationForm(data: any): void {
    if (!data) return;

    this.legalFullName = data.legalFullName ?? '';
    this.dateOfBirth = data.dateOfBirth ? String(data.dateOfBirth).substring(0, 10) : '';
    this.addressLine1 = data.addressLine1 ?? '';
    this.addressLine2 = data.addressLine2 ?? '';
    this.city = data.city ?? '';
    this.stateCode = data.stateCode ?? '';
    this.zipCode = data.zipCode ?? '';
    this.phoneNumber = data.phoneNumber ?? '';
    this.jurisdictionId = data.jurisdictionId ?? null;
    this.idDocumentType = data.idDocumentType ?? '';
    this.idDocumentNumberMasked = data.idDocumentNumberMasked ?? '';
    this.idDocumentState = data.idDocumentState ?? '';
    this.userEmail = data.email ?? this.userEmail;
    this.userId = data.userId ? String(data.userId) : this.userId;
    this.idPicturePath = data.idPicturePath ?? '';
    this.age = this.calculateAge(this.dateOfBirth);
  }

  canAccessVoting(): boolean {
    return this.verificationRecord?.isIdentityVerified === true
      && this.verificationRecord?.isResidenceVerified === true
      && this.verificationRecord?.isEligibleToVote === true
      && this.verificationRecord?.verificationStatus === 'Approved';
  }

  getStatusClass(): string {
    const status = (this.verificationStatus || '').toLowerCase();

    if (status === 'approved') return 'status-approved';
    if (status === 'pending') return 'status-pending';
    if (status === 'rejected') return 'status-rejected';
    return 'status-default';
  }

  getFullImageUrl(): string {
    if (!this.idPicturePath) return '';

    if (this.idPicturePath.startsWith('http://') || this.idPicturePath.startsWith('https://')) {
      return this.idPicturePath;
    }

    return `${this.apiBaseUrl}${this.idPicturePath}`;
  }

  submitVerification(): void {
    this.error = '';
    this.success = '';

    if (!this.legalFullName.trim()) {
      this.error = 'Legal full name is required.';
      this.cdr.detectChanges();
      return;
    }

    if (
      !this.addressLine1.trim() ||
      !this.city.trim() ||
      !this.stateCode.trim() ||
      !this.zipCode.trim()
    ) {
      this.error = 'Address fields are required.';
      this.cdr.detectChanges();
      return;
    }

    if (!this.phoneNumber.trim()) {
      this.error = 'Phone number is required.';
      this.cdr.detectChanges();
      return;
    }

    if (!this.idDocumentType.trim()) {
      this.error = 'ID document type is required.';
      this.cdr.detectChanges();
      return;
    }

    this.verificationSaving = true;
    this.cdr.detectChanges();

    const payload = {
      legalFullName: this.legalFullName.trim(),
      dateOfBirth: this.dateOfBirth || null,
      addressLine1: this.addressLine1.trim(),
      addressLine2: this.addressLine2.trim() || null,
      city: this.city.trim(),
      stateCode: this.stateCode.trim(),
      zipCode: this.zipCode.trim(),
      jurisdictionId: this.jurisdictionId,
      phoneNumber: this.phoneNumber.trim(),
      idDocumentType: this.idDocumentType.trim(),
      idDocumentNumberMasked: this.idDocumentNumberMasked.trim() || null,
      idDocumentState: this.idDocumentState.trim() || null
    };

    this.verificationSvc.submit(payload)
      .pipe(finalize(() => {
        this.verificationSaving = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (res: any) => {
          this.success = res?.message ?? 'Verification submitted successfully.';
          this.verificationStatus = 'Pending';
          this.verificationRecord = {
            ...(this.verificationRecord || {}),
            verificationStatus: 'Pending',
            isIdentityVerified: false,
            isResidenceVerified: false,
            isEligibleToVote: false
          };
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          this.error = err?.error?.message ?? 'Failed to submit verification.';
          this.cdr.detectChanges();
        }
      });
  }

  calculateAge(dateOfBirth: string): number | null {
    if (!dateOfBirth) return null;

    const dob = new Date(dateOfBirth);
    if (isNaN(dob.getTime())) return null;

    const today = new Date();

    if (dob > today) return null;

    let age = today.getFullYear() - dob.getFullYear();
    const monthDiff = today.getMonth() - dob.getMonth();

    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < dob.getDate())) {
      age--;
    }

    return age >= 0 ? age : null;
  }

  getFormattedAddress(): string {
    const parts = [
      this.addressLine1,
      this.addressLine2,
      this.city,
      this.stateCode,
      this.zipCode
    ].filter(p => p && p.trim() !== '');

    return parts.join(', ');
  }
}