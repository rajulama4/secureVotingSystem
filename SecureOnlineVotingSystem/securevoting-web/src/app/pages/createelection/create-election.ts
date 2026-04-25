import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ElectionsService } from '../../services/elections';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-create-election',
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
  templateUrl: './create-election.html',
  styleUrl: './create-election.css'
})
export class CreateElectionComponent implements OnInit {
  title = '';
  description = '';
  startTime = '';
  endTime = '';

  jurisdictions: any[] = [];
  jurisdictionId: number | null = null;

  saving = false;
  error = '';
  success = '';

  constructor(
    private electionsSvc: ElectionsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadJurisdictions();
  }

  loadJurisdictions() {
    this.electionsSvc.getJurisdictions().subscribe({
      next: (res: any[]) => {
        console.log('JURISDICTIONS:', res);
        this.jurisdictions = res;
      },
      error: (err) => {
        console.error('Failed to load jurisdictions', err);
        this.error = 'Failed to load jurisdictions.';
      }
    });
  }

  createElection() {
    this.error = '';
    this.success = '';

    if (!this.title.trim()) {
      this.error = 'Title is required.';
      return;
    }

    if (!this.startTime || !this.endTime) {
      this.error = 'Start time and end time are required.';
      return;
    }

    if (!this.jurisdictionId) {
      this.error = 'Jurisdiction is required.';
      return;
    }

    if (new Date(this.endTime) <= new Date(this.startTime)) {
      this.error = 'End time must be after start time.';
      return;
    }

    this.saving = true;

    this.electionsSvc.create({
      title: this.title.trim(),
      description: this.description.trim(),
      startTime: new Date(this.startTime).toISOString(),
      endTime: new Date(this.endTime).toISOString(),
      jurisdictionId: this.jurisdictionId
    }).subscribe({
      next: (res) => {
        console.log('CREATE RESPONSE:', res);

        this.success = res?.message ?? 'Election created successfully.';

        this.title = '';
        this.description = '';
        this.startTime = '';
        this.endTime = '';
        this.jurisdictionId = null;

        this.saving = false;

        setTimeout(() => {
          this.router.navigate(['/admin/manage-elections'], {
            queryParams: { msg: this.success }
          });
        }, 1500);
      },
      error: (err) => {
        console.error('CREATE ERROR:', err);
        this.error = err?.error?.message ?? 'Failed to create election.';
        this.saving = false;
      }
    });
  }
}