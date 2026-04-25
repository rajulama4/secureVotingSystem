import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

export type LoginApiResponse =
  | { message: 'Login successful.'; token: string }
  | { message: 'TOTP_REQUIRED'; email: string }
  | { message: 'TOTP_ENROLL_REQUIRED'; email: string; otpauthUri: string; qrCodeBase64Png: string }
  | { message: 'PASSWORD_CHANGE_REQUIRED'; userId: number; email: string; loginUserId?: string }
  | { message: string; [key: string]: any };

export type VerifyTotpResponse =
  | { message: 'Login successful.'; token: string }
  | { message: 'INVALID_TOTP' }
  | { message: 'TOTP_NOT_ENROLLED' }
  | { message: string; [key: string]: any };

// (Optional) keep old MFA types if you still have /verify-mfa in API
export interface VerifyMfaResponse {
  message: string;
  token?: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private base = environment.apiUrl; // example: https://localhost:7131

  constructor(private http: HttpClient) {}

  // NEW: Login starts TOTP flow
login(loginId: string, password: string) {
  return this.http.post<LoginApiResponse>(`${this.base}/api/auth/login`, { loginId, password });
}

  // NEW: Verify TOTP code and get JWT
  verifyTotp(email: string, code: string) {
    return this.http.post<VerifyTotpResponse>(`${this.base}/api/auth/verify-totp`, { email, code });
  }

  // OPTIONAL: keep old endpoint if you still use it anywhere
  verifyMfa(userId: number, challengeId: number, code: string) {
    return this.http.post<VerifyMfaResponse>(`${this.base}/api/auth/verify-mfa`, { userId, challengeId, code });
  }

     

  registerVoter(data: FormData) {
  return this.http.post(`${this.base}/api/auth/register-voter`, data);
}

  changeTempPassword(userId: number, newPassword: string) {
  return this.http.post<any>(`${this.base}/api/auth/change-temp-password`, {
    userId,
    newPassword
  });
}    

}