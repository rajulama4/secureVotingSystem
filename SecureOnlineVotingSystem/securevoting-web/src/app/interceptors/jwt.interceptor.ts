import { HttpInterceptorFn } from '@angular/common/http';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const isPublicAuthEndpoint =
    req.url.includes('/api/auth/login') ||
    req.url.includes('/api/auth/register-voter') ||
    req.url.includes('/api/auth/verify-totp') ||
    req.url.includes('/api/auth/verify-mfa');

  if (isPublicAuthEndpoint) {
    return next(req);
  }

  const token = sessionStorage.getItem('token');

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req);
};