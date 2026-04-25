import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../services/auth';

import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatFormFieldModule
  ],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css'
})
export class ChangePasswordComponent {
  newPassword = '';
  confirmPassword = '';
  error = '';
  success = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  submit() {
    this.error = '';
    this.success = '';

    const userId = Number(sessionStorage.getItem('pw_change_userId') || '0');

    if (!userId) {
      this.error = 'Password change session not found.';
      return;
    }

    if (!this.newPassword || !this.confirmPassword) {
      this.error = 'Please enter and confirm your new password.';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.error = 'Passwords do not match.';
      return;
    }

    this.loading = true;

    this.auth.changeTempPassword(userId, this.newPassword)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (res) => {
          this.success = res?.message || 'Password changed successfully.';
          sessionStorage.removeItem('pw_change_userId');
          sessionStorage.removeItem('pw_change_email');

          setTimeout(() => this.router.navigate(['/login']), 1200);
        },
        error: (err) => {
          this.error = err?.error?.message || 'Password change failed.';
        }
      });
  }
}