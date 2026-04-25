import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenService {

  setToken(token: string) {
    sessionStorage.setItem('token', token);
  }

  getToken(): string | null {
    return sessionStorage.getItem('token');
  }

  clear() {
    sessionStorage.removeItem('token');
  }

  getRole(token: string): string | null {
    try {
      const payload = token.split('.')[1];
      const json = JSON.parse(atob(payload));
      return json['role']
        ?? json['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        ?? null;
    } catch {
      return null;
    }
  }
}
