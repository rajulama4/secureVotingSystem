import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { TokenService } from '../../services/token';


@Component({
  selector: 'app-totp',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './totp.html',
  styleUrl: './totp.css',
})
export class TotpComponent {
  email = '';
  code = '';
  error = '';
  loading = false;

  // for enrollment UI
  enrollMessage = '';
  qrBase64 = '';
  otpauthUri = '';
  isEnrollMode = false;

  constructor(
    private auth: AuthService,
    private tokenSvc: TokenService,
    private router: Router
  ) {
    // email passed from login
    const savedEmail = sessionStorage.getItem('totp_email');
    if (savedEmail) this.email = savedEmail;

    // enrollment info (optional)
    const qr = sessionStorage.getItem('totp_qr');
    const uri = sessionStorage.getItem('totp_uri');
    if (qr || uri) {
      this.isEnrollMode = true;
      this.qrBase64 = qr ?? '';
      this.otpauthUri = uri ?? '';
      this.enrollMessage = 'Scan the QR code in Google Authenticator / Microsoft Authenticator, then enter the 6-digit code.';
    }
  }

  verify() {
    this.error = '';
    if (!this.email) {
      this.error = 'Missing email. Go back to login.';
      return;
    }
    if (!this.code || this.code.trim().length < 6) {
      this.error = 'Enter the 6-digit code.';
      return;
    }

    this.loading = true;
    this.auth.verifyTotp(this.email, this.code.trim()).subscribe({
    next: (res) => {
  this.loading = false;

  // ✅ type narrowing
  if (res.message !== 'Login successful.') {
    this.error =
      res.message === 'INVALID_TOTP' ? 'Invalid code. Try again.' :
      res.message === 'TOTP_NOT_ENROLLED' ? 'TOTP not enrolled. Please login again.' :
      res.message;

    return;
  }

  // ✅ now TS knows token exists
  const token = res.token;

  this.tokenSvc.setToken(token);

  sessionStorage.removeItem('totp_email');
  sessionStorage.removeItem('totp_qr');
  sessionStorage.removeItem('totp_uri');

  this.redirectByRole(token);
},
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? 'Invalid code.';
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