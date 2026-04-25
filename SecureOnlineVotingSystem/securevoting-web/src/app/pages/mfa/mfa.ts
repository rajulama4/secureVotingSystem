import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { TokenService } from '../../services/token';

@Component({
  selector: 'app-mfa',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './mfa.html',
  styleUrl: './mfa.css',
})
export class MfaComponent implements OnInit {
  code = '';
  error = '';
  loading = false;

  userId = 0;
  challengeId = 0;
  simCode = '';

  constructor(
    private auth: AuthService,
    private tokenSvc: TokenService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userId = Number(sessionStorage.getItem('mfa_userId') ?? '0');
    this.challengeId = Number(sessionStorage.getItem('mfa_challengeId') ?? '0');
    this.simCode = sessionStorage.getItem('mfa_sim_code') ?? '';

    if (!this.userId || !this.challengeId) {
      this.router.navigate(['/login']);
    }
  }

  verify() {
    this.error = '';
    this.loading = true;

    this.auth.verifyMfa(this.userId, this.challengeId, this.code).subscribe({
      next: (res) => {
        this.loading = false;

        console.log('verify-mfa response:', res);

        if (!res.token) {
          this.error = 'No token returned from server.';
          return;
        }

        this.tokenSvc.setToken(res.token);

        // cleanup temp MFA data
        sessionStorage.removeItem('mfa_userId');
        sessionStorage.removeItem('mfa_challengeId');
        sessionStorage.removeItem('mfa_sim_code');

        const role = this.tokenSvc.getRole(res.token);
        if (role === 'Admin') this.router.navigate(['/admin']);
        else if (role === 'Voter') this.router.navigate(['/voter']);
        else this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading = false;
            this.error =
        err?.error?.message ??
        (typeof err?.error === 'string' ? err.error : null) ??
        err?.message ??
        'MFA verification failed.';
      }
    });
  }
}
