import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { ElectionsService } from '../../services/elections';
import { CandidatesService } from '../../services/candidates';
import { environment } from '../../../environments/environment';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-manage-candidates',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule
  ],
  templateUrl: './manage-candidates.html',
  styleUrls: ['./manage-candidates.css']
})
export class ManageCandidatesComponent implements OnInit {
  jurisdictions: any[] = [];
  elections: any[] = [];
  filteredElections: any[] = [];
  candidates: any[] = [];

  jurisdictionId: number | null = null;
  electionId: number | null = null;

  candidateName = '';
  party = '';
  bio = '';

  selectedCandidateFile: File | null = null;
  candidateFileError = '';

  editingCandidateId: number | null = null;
  editCandidateName = '';
  editParty = '';
  editBio = '';
  editSelectedFile: File | null = null;
  editFileError = '';
  currentEditImagePath = '';

  loading = false;
  saving = false;
  updating = false;
  loadingJurisdictions = false;

  error = '';
  success = '';

  apiBaseUrl = environment.apiUrl;

  constructor(
    private electionsSvc: ElectionsService,
    private candidatesSvc: CandidatesService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadJurisdictions();
    this.loadElections();
  }

  loadJurisdictions() {
    this.loadingJurisdictions = true;
    this.error = '';
    this.cdr.detectChanges();

    this.electionsSvc.getJurisdictions().subscribe({
      next: (res: any[]) => {
        this.jurisdictions = res || [];
        this.loadingJurisdictions = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load jurisdictions', err);
        this.error = 'Failed to load jurisdictions.';
        this.loadingJurisdictions = false;
        this.cdr.detectChanges();
      }
    });
  }

  loadElections() {
    this.error = '';

    this.electionsSvc.getAll().subscribe({
      next: (res: any[]) => {
        this.elections = res || [];
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Failed to load elections', err);
        this.error = err?.error?.message ?? 'Failed to load elections.';
        this.cdr.detectChanges();
      }
    });
  }

  applyElectionFilter() {
    if (!this.jurisdictionId) {
      this.filteredElections = [];
      this.electionId = null;
      this.candidates = [];
      this.cancelEditCandidate();
      this.cdr.detectChanges();
      return;
    }

    const selectedId = Number(this.jurisdictionId);

    this.filteredElections = this.elections.filter(
      e => Number(e.jurisdictionId) === selectedId
    );

    this.electionId = null;
    this.candidates = [];
    this.cancelEditCandidate();
    this.cdr.detectChanges();
  }

  onElectionChange() {
    this.cancelEditCandidate();

    if (!this.electionId) {
      this.candidates = [];
      this.cdr.detectChanges();
      return;
    }

    this.loadCandidates();
  }

  loadCandidates() {
    if (!this.electionId) {
      this.candidates = [];
      this.cdr.detectChanges();
      return;
    }

    this.loading = true;
    this.error = '';
    this.cdr.detectChanges();

    this.candidatesSvc.getByElection(this.electionId).subscribe({
      next: (res: any[]) => {
        this.candidates = res || [];
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('LOAD CANDIDATES ERROR:', err);
        this.error = err?.error?.message ?? 'Failed to load candidates.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  onCandidateFileSelected(event: Event) {
    this.candidateFileError = '';

    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      this.selectedCandidateFile = null;
      return;
    }

    const file = input.files[0];
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];

    if (!allowedTypes.includes(file.type)) {
      this.selectedCandidateFile = null;
      this.candidateFileError = 'Only JPG, PNG, and WEBP files are allowed.';
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.selectedCandidateFile = null;
      this.candidateFileError = 'Max file size is 5MB.';
      return;
    }

    this.selectedCandidateFile = file;
  }

  createCandidate() {
    this.error = '';
    this.success = '';
    this.candidateFileError = '';

    if (!this.jurisdictionId) {
      this.error = 'Jurisdiction is required.';
      this.cdr.detectChanges();
      return;
    }

    if (!this.electionId) {
      this.error = 'Election is required.';
      this.cdr.detectChanges();
      return;
    }

    if (!this.candidateName?.trim()) {
      this.error = 'Candidate name is required.';
      this.cdr.detectChanges();
      return;
    }

    this.saving = true;
    this.cdr.detectChanges();

    const formData = new FormData();
    formData.append('ElectionId', this.electionId.toString());
    formData.append('CandidateName', this.candidateName.trim());
    formData.append('Party', this.party?.trim() || '');
    formData.append('Bio', this.bio?.trim() || '');

    if (this.selectedCandidateFile) {
      formData.append('CandidatePicture', this.selectedCandidateFile);
    }

    this.candidatesSvc.create(formData).subscribe({
      next: (res: any) => {
        this.success = res?.message ?? 'Candidate created successfully.';

        this.candidateName = '';
        this.party = '';
        this.bio = '';
        this.selectedCandidateFile = null;
        this.candidateFileError = '';

        this.saving = false;
        this.cdr.detectChanges();

        setTimeout(() => {
          this.loadCandidates();
        }, 0);

        setTimeout(() => {
          this.success = '';
          this.cdr.detectChanges();
        }, 2000);
      },
      error: (err) => {
        console.error('CREATE ERROR:', err);
        this.error = err?.error?.message ?? 'Failed to create candidate.';
        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }

  startEditCandidate(c: any) {
    this.error = '';
    this.success = '';

    this.editingCandidateId = c.candidateId;
    this.editCandidateName = c.candidateName || '';
    this.editParty = c.party || '';
    this.editBio = c.bio || '';
    this.currentEditImagePath = c.candidateImagePath || '';
    this.editSelectedFile = null;
    this.editFileError = '';

    this.cdr.detectChanges();
  }

  cancelEditCandidate() {
    this.editingCandidateId = null;
    this.editCandidateName = '';
    this.editParty = '';
    this.editBio = '';
    this.currentEditImagePath = '';
    this.editSelectedFile = null;
    this.editFileError = '';
    this.updating = false;
    this.cdr.detectChanges();
  }

  onEditCandidateFileSelected(event: Event) {
    this.editFileError = '';

    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      this.editSelectedFile = null;
      return;
    }

    const file = input.files[0];
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];

    if (!allowedTypes.includes(file.type)) {
      this.editSelectedFile = null;
      this.editFileError = 'Only JPG, PNG, and WEBP files are allowed.';
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.editSelectedFile = null;
      this.editFileError = 'Max file size is 5MB.';
      return;
    }

    this.editSelectedFile = file;
  }

  updateCandidate() {
    this.error = '';
    this.success = '';
    this.editFileError = '';

    if (!this.editingCandidateId) {
      this.error = 'No candidate selected.';
      this.cdr.detectChanges();
      return;
    }

    if (!this.editCandidateName?.trim()) {
      this.error = 'Candidate name is required.';
      this.cdr.detectChanges();
      return;
    }

    this.updating = true;
    this.cdr.detectChanges();

    const formData = new FormData();
    formData.append('CandidateId', this.editingCandidateId.toString());
    formData.append('CandidateName', this.editCandidateName.trim());
    formData.append('Party', this.editParty?.trim() || '');
    formData.append('Bio', this.editBio?.trim() || '');

    if (this.editSelectedFile) {
      formData.append('CandidatePicture', this.editSelectedFile);
    }

    this.candidatesSvc.update(formData).subscribe({
      next: (res: any) => {
        this.success = res?.message ?? 'Candidate updated successfully.';
        this.updating = false;
        this.cancelEditCandidate();
        this.loadCandidates();

        setTimeout(() => {
          this.success = '';
          this.cdr.detectChanges();
        }, 2000);
      },
      error: (err) => {
        console.error('UPDATE ERROR:', err);
        this.error = err?.error?.message ?? 'Failed to update candidate.';
        this.updating = false;
        this.cdr.detectChanges();
      }
    });
  }

  deactivateCandidate(candidateId: number) {
    this.error = '';
    this.success = '';

    this.candidatesSvc.deactivate(candidateId).subscribe({
      next: (res: any) => {
        this.success = res?.message ?? 'Candidate deactivated successfully.';
        this.loadCandidates();

        setTimeout(() => {
          this.success = '';
          this.cdr.detectChanges();
        }, 2000);
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to deactivate candidate.';
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