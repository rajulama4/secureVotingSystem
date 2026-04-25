import { Component, Inject, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiLogsService, ApiLogRow, ApiLogDetail } from '../../services/api-logs';

import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import {
  MatDialog,
  MatDialogModule,
  MAT_DIALOG_DATA,
  MatDialogRef
} from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-api-logs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatInputModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatSelectModule,
    MatTooltipModule
  ],
  templateUrl: './api-logs.html',
  styleUrl: './api-logs.css'
})
export class ApiLogsComponent implements OnInit {
  rows: ApiLogRow[] = [];
  loading = false;
  error = '';

  // filters
  email = '';
  endpoint = '';
  userIdText = '';

  // paging
  pageSize = 10;
  currentPage = 0;
  totalCount = 0;
  skip = 0;

  // Page size options
  pageSizeOptions = [5, 10, 25, 50, 100];

  displayedColumns = ['time', 'method', 'endpoint', 'email', 'userId', 'status', 'duration', 'actions'];

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private api: ApiLogsService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    // Small delay to ensure view is initialized
    setTimeout(() => {
      this.load();
    }, 0);
  }

  load() {
    this.loading = true;
    this.error = '';

    const userId = this.userIdText.trim() ? Number(this.userIdText.trim()) : undefined;

    // Calculate skip based on current page and page size
    this.skip = this.currentPage * this.pageSize;

    this.api.list({
      take: this.pageSize,
      skip: this.skip,
      email: this.email.trim() || undefined,
      endpoint: this.endpoint.trim() || undefined,
      userId: Number.isFinite(userId as any) ? userId : undefined
    }).subscribe({
      next: (data) => {
        // Handle both array response and paginated response
        if (Array.isArray(data)) {
          this.rows = data;
          // If API doesn't return total count, estimate it
          if (data.length === this.pageSize) {
            // If we got a full page, assume there might be more
            this.totalCount = this.skip + this.pageSize + 1;
          } else {
            // If we got less than page size, this is the last page
            this.totalCount = this.skip + data.length;
          }
        } else if (data && typeof data === 'object') {
          // If API returns paginated object with items and totalCount
          const response = data as any;
          this.rows = response.items || response.data || [];
          this.totalCount = response.totalCount || response.total || this.rows.length;
        } else {
          this.rows = [];
          this.totalCount = 0;
        }
        
        this.loading = false;
        // Force change detection
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading logs:', err);
        this.loading = false;
        this.error = err?.error?.message || err?.message || 'Failed to load logs.';
        // Force change detection even on error
        this.cdr.detectChanges();
      }
    });
  }

  // Handle page change from paginator
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.currentPage = event.pageIndex;
    this.load();
  }

  // Helper method to get page info
  getPageInfo(): string {
    if (this.totalCount === 0) {
      return 'No logs found';
    }
    const start = (this.currentPage * this.pageSize) + 1;
    const end = Math.min((this.currentPage + 1) * this.pageSize, this.totalCount);
    return `Showing ${start} - ${end} of ${this.totalCount} log${this.totalCount !== 1 ? 's' : ''}`;
  }

  openDetail(row: ApiLogRow) {
    this.dialog.open(ApiLogDetailDialog, {
      width: '900px',
      maxWidth: '95vw',
      data: { logId: row.logId },
      autoFocus: false
    });
  }

  resetFilters() {
    this.email = '';
    this.endpoint = '';
    this.userIdText = '';
    this.currentPage = 0;
    this.load();
  }

  // Helper to get status class
  getStatusClass(isSuccess: boolean): string {
    return isSuccess ? 'status-success' : 'status-error';
  }

  // Helper to get method class
  getMethodClass(method: string): string {
    switch (method) {
      case 'GET': return 'method-get';
      case 'POST': return 'method-post';
      case 'PUT': return 'method-put';
      case 'DELETE': return 'method-delete';
      default: return 'method-other';
    }
  }
}

@Component({
  selector: 'app-api-log-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="dialog-container">
      <div class="dialog-header">
        <div class="dialog-title">
          <mat-icon>assignment</mat-icon>
          <span>API Log Detail #{{ data.logId }}</span>
        </div>
        <button mat-icon-button type="button" (click)="close()" matTooltip="Close">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div *ngIf="loading" class="dialog-loading">
        <mat-spinner diameter="40"></mat-spinner>
        <span>Loading details...</span>
      </div>

      <div *ngIf="error" class="dialog-error">
        <mat-icon color="warn">error</mat-icon>
        <span>{{ error }}</span>
        <button mat-button color="primary" (click)="close()">Close</button>
      </div>

      <div *ngIf="detail && !loading" class="dialog-content">
        <mat-card class="info-card">
          <div class="info-grid">
            <div class="info-item">
              <span class="info-label">Log ID:</span>
              <span class="info-value">{{ detail.logId }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Time:</span>
              <span class="info-value">{{ detail.requestTimeUtc | date:'medium' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Method:</span>
              <span class="info-value method-badge" [class]="getMethodClass(detail.httpMethod)">
                {{ detail.httpMethod }}
              </span>
            </div>
            <div class="info-item">
              <span class="info-label">Endpoint:</span>
              <span class="info-value">{{ detail.endpoint }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">User:</span>
              <span class="info-value">{{ detail.email || '-' }} (ID: {{ detail.userId ?? '-' }})</span>
            </div>
            <div class="info-item">
              <span class="info-label">Status:</span>
              <span class="info-value">
                <span class="status-badge" [class.status-success]="detail.isSuccess" [class.status-error]="!detail.isSuccess">
                  {{ detail.statusCode }} {{ detail.isSuccess ? '✓' : '✗' }}
                </span>
              </span>
            </div>
            <div class="info-item">
              <span class="info-label">Duration:</span>
              <span class="info-value">{{ detail.durationMs ?? '-' }} ms</span>
            </div>
            <div class="info-item">
              <span class="info-label">IP Address:</span>
              <span class="info-value">{{ detail.ipAddress || '-' }}</span>
            </div>
            <div class="info-item full-width">
              <span class="info-label">User Agent:</span>
              <span class="info-value">{{ detail.userAgent || '-' }}</span>
            </div>
          </div>
        </mat-card>

        <mat-card class="data-card">
          <div class="data-header">
            <mat-icon>request_quote</mat-icon>
            <span>Request</span>
          </div>
          <pre class="json">{{ pretty(detail.apiReq) }}</pre>
        </mat-card>

        <mat-card class="data-card">
          <div class="data-header">
            <mat-icon>response</mat-icon>
            <span>Response</span>
          </div>
          <pre class="json">{{ pretty(detail.apiRes) }}</pre>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dialog-container {
      padding: 20px;
      min-width: 600px;
      max-width: 900px;
    }
    .dialog-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 20px;
    }
    .dialog-title {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 20px;
      font-weight: 500;
      color: #333;
    }
    .dialog-loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 48px;
      gap: 16px;
      color: #666;
    }
    .dialog-error {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 24px;
      gap: 8px;
      color: #crimson;
      background-color: #fff3f3;
      border-radius: 4px;
    }
    .dialog-content {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    .info-card {
      padding: 16px;
      background-color: #f8f9fa;
    }
    .info-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 12px;
    }
    .info-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
    .info-item.full-width {
      grid-column: 1 / -1;
    }
    .info-label {
      font-size: 12px;
      color: #666;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .info-value {
      font-size: 14px;
      color: #333;
      word-break: break-word;
    }
    .method-badge {
      display: inline-block;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
    }
    .method-get { background-color: #e3f2fd; color: #1976d2; }
    .method-post { background-color: #e8f5e9; color: #388e3c; }
    .method-put { background-color: #fff3e0; color: #f57c00; }
    .method-delete { background-color: #ffebee; color: #d32f2f; }
    .method-other { background-color: #f5f5f5; color: #666; }
    
    .status-badge {
      display: inline-block;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
    }
    .status-success {
      background-color: #e8f5e9;
      color: #388e3c;
    }
    .status-error {
      background-color: #ffebee;
      color: #d32f2f;
    }
    .data-card {
      padding: 16px;
    }
    .data-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 12px;
      font-weight: 500;
      color: #333;
    }
    .json {
      white-space: pre-wrap;
      word-break: break-word;
      background: #0b1020;
      color: #e6e6e6;
      border-radius: 8px;
      padding: 16px;
      font-size: 13px;
      line-height: 1.5;
      font-family: 'Monaco', 'Menlo', 'Courier New', monospace;
      max-height: 400px;
      overflow: auto;
      margin: 0;
    }
    @media (max-width: 768px) {
      .dialog-container {
        padding: 16px;
        min-width: auto;
      }
      .info-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ApiLogDetailDialog implements OnInit {
  loading = true;
  error = '';
  detail?: ApiLogDetail;

  constructor(
    private api: ApiLogsService,
    private dialogRef: MatDialogRef<ApiLogDetailDialog>,
    private cdr: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) public data: { logId: number }
  ) {}

  // ✅ FIX: move API call here
  ngOnInit(): void {
    this.loadDetail();
  }

  loadDetail() {
    this.loading = true;
    this.error = '';

    this.api.detail(this.data.logId).subscribe({
      next: (d) => {
        this.detail = d;

        // ✅ FIX: avoid ExpressionChanged error
        setTimeout(() => {
          this.loading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        console.error('Error loading detail:', err);

        setTimeout(() => {
          this.error = err?.error?.message || err?.message || 'Failed to load detail.';
          this.loading = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  close() {
    this.dialogRef.close();
  }

  pretty(text?: string | null): string {
    if (!text) return 'No data';
    try {
      const obj = JSON.parse(text);
      return JSON.stringify(obj, null, 2);
    } catch {
      return text;
    }
  }

  getMethodClass(method: string): string {
    switch (method) {
      case 'GET': return 'method-get';
      case 'POST': return 'method-post';
      case 'PUT': return 'method-put';
      case 'DELETE': return 'method-delete';
      default: return 'method-other';
    }
  }
}