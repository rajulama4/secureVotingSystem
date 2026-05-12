import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { AuthService } from '../../services/auth';
import { TokenService } from '../../services/token';

import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-totp',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './totp.html',
  styleUrl: './totp.css',
})
export class TotpComponent {
  email = '';
  code = '';
  error = '';
  loading = false;

  enrollMessage = '';
  qrBase64 = '';
  otpauthUri = '';
  isEnrollMode = false;

  constructor(
    private auth: AuthService,
    private tokenSvc: TokenService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    const savedEmail = sessionStorage.getItem('totp_email');
    if (savedEmail) this.email = savedEmail;

    const qr = sessionStorage.getItem('totp_qr');
    const uri = sessionStorage.getItem('totp_uri');

    if (qr || uri) {
      this.isEnrollMode = true;
      this.qrBase64 = qr ?? '';
      this.otpauthUri = uri ?? '';
      this.enrollMessage =
        'Scan the QR code in Google Authenticator or Microsoft Authenticator, then enter the 6-digit code.';
    }
  }

  verify() {
    this.error = '';

    if (!this.email) {
      this.error = 'Missing email. Go back to login.';
      this.cdr.detectChanges();
      return;
    }

    if (!this.code || this.code.trim().length < 6) {
      this.error = 'Enter the 6-digit code.';
      this.cdr.detectChanges();
      return;
    }

    this.loading = true;
    this.cdr.detectChanges();

    this.auth.verifyTotp(this.email, this.code.trim())
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (res: any) => {
          if (res?.message !== 'Login successful.') {
            this.error =
              res?.message === 'INVALID_TOTP'
                ? 'Invalid code. Try again.'
                : res?.message === 'TOTP_NOT_ENROLLED'
                ? 'TOTP not enrolled. Please login again.'
                : res?.message || 'Verification failed.';

            this.cdr.detectChanges();
            return;
          }

          const token = res.token;
          this.tokenSvc.setToken(token);

          sessionStorage.removeItem('totp_email');
          sessionStorage.removeItem('totp_qr');
          sessionStorage.removeItem('totp_uri');

          this.redirectByRole(token);
        },

        error: (err) => {
          console.error('TOTP error:', err);

          if (typeof err?.error === 'string') {
            this.error = err.error;
          } else {
            this.error =
              err?.error?.message ||
              err?.message ||
              'Verification failed.';
          }

          this.cdr.detectChanges();
        }
      });
  }

  private redirectByRole(token: string) {
    const role = this.tokenSvc.getRole(token);

    if (role === 'Admin') this.router.navigate(['/admin']);
    else if (role === 'Voter') this.router.navigate(['/voter']);
    else this.router.navigate(['/login']);
  }
}