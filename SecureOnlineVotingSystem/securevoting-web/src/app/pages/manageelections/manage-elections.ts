import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ElectionsService, ElectionRow } from '../../services/elections';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator'; // Add this import

@Component({
  selector: 'app-manage-elections',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule // Add this
  ],
  templateUrl: './manage-elections.html',
  styleUrl: './manage-elections.css'
})
export class ManageElectionsComponent implements OnInit {
  // All elections data
  allElections: ElectionRow[] = [];
  
  // Paginated elections to display
  paginatedElections: ElectionRow[] = [];
  
  // Pagination properties
  pageSize = 10;
  currentPage = 0;
  totalElections = 0;
  
  // UI state
  loading = false;
  error = '';
  success = '';

  constructor(
    private electionsSvc: ElectionsService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['msg']) {
        this.success = params['msg'];
        setTimeout(() => {
          this.success = '';
          this.cdr.detectChanges();
        }, 3000);
      }
    });

    this.loadElections();
  }

  loadElections(): void {
    this.loading = true;
    this.error = '';
    
    this.electionsSvc.getAll().subscribe({
      next: (data: any) => {
        this.allElections = Array.isArray(data) ? data : [];
        this.totalElections = this.allElections.length;
        
        // Load first page
        this.updatePaginatedData();
        
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.error = err?.error?.message || err?.message || 'Failed to load elections.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Update the data for current page
  updatePaginatedData(): void {
    const startIndex = this.currentPage * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedElections = this.allElections.slice(startIndex, endIndex);
  }

  // Handle page change
  onPageChange(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.currentPage = event.pageIndex;
    this.updatePaginatedData();
    this.cdr.detectChanges();
  }

  closeElection(row: ElectionRow): void {
    this.error = '';
    this.success = '';

    this.electionsSvc.closeElection(row.electionId).subscribe({
      next: (res) => {
        this.success = res?.message ?? 'Election closed successfully.';
        this.cdr.detectChanges();
        this.loadElections(); // Reload all data
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to close election.';
        this.cdr.detectChanges();
      }
    });
  }

  publishResults(row: ElectionRow): void {
    this.error = '';
    this.success = '';

    this.electionsSvc.publishResults(row.electionId).subscribe({
      next: (res) => {
        this.success = res?.message ?? 'Results published successfully.';
        this.cdr.detectChanges();
        this.loadElections(); // Reload all data
      },
      error: (err) => {
        this.error = err?.error?.message ?? 'Failed to publish results.';
        this.cdr.detectChanges();
      }
    });
  }

  // Helper method to get page range display
  getShowingRange(): string {
    const start = this.currentPage * this.pageSize + 1;
    const end = Math.min((this.currentPage + 1) * this.pageSize, this.totalElections);
    return `Showing ${start} - ${end} of ${this.totalElections} elections`;
  }
}