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
  selector: 'app-login',
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
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  loginId = '';
  password = '';
  error = '';
  loading = false;
  hidePassword = true;

  constructor(
    private auth: AuthService,
    private tokenSvc: TokenService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  submit() {
    this.error = '';

    const loginId = this.loginId?.trim() ?? '';
    const password = this.password?.trim() ?? '';

    if (!loginId && !password) {
      this.error = 'Email or User ID and Password are required.';
      this.cdr.detectChanges();
      return;
    }
    if (!loginId) {
      this.error = 'Email or User ID is required.';
      this.cdr.detectChanges();
      return;
    }
    if (!password) {
      this.error = 'Password is required.';
      this.cdr.detectChanges();
      return;
    }

    this.loading = true;
    this.cdr.detectChanges();

    this.auth.login(loginId, password)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (res: any) => {
          console.log('Login response:', res);

          if (res?.token) {
            this.tokenSvc.setToken(res.token);
            this.redirectByRole(res.token);
            return;
          }
            if (res?.message === 'PASSWORD_CHANGE_REQUIRED') {
              sessionStorage.setItem('pw_change_userId', String(res.userId));
              sessionStorage.setItem('pw_change_email', res.email ?? '');
              this.router.navigate(['/change-password']);
              return;
            }

          if (res?.message === 'TOTP_ENROLL_REQUIRED') {
            sessionStorage.setItem('totp_email', res.email ?? '');
            sessionStorage.setItem('totp_qr', res.qrCodeBase64Png ?? '');
            sessionStorage.setItem('totp_uri', res.otpauthUri ?? '');
            this.router.navigate(['/totp']);
            return;
          }

          if (res?.message === 'TOTP_REQUIRED') {
            sessionStorage.setItem('totp_email', res.email ?? '');
            sessionStorage.removeItem('totp_qr');
            sessionStorage.removeItem('totp_uri');
            this.router.navigate(['/totp']);
            return;
          }

          this.error = res?.message || 'Unexpected response from server.';
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Login error:', err);
          console.log('err.error:', err?.error);

          if (typeof err?.error === 'string') {
            this.error = err.error;
          } else {
            this.error =
              err?.error?.message ||
              err?.message ||
              'Invalid email/user ID or password.';
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