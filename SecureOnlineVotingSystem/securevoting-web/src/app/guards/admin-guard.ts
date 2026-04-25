import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { TokenService } from '../services/token';

export const adminGuard: CanActivateFn = () => {
  const tokenSvc = inject(TokenService);
  const router = inject(Router);

  const token = tokenSvc.getToken();
  if (!token) {
    router.navigate(['/login']);
    return false;
  }

  const role = tokenSvc.getRole(token);
  if (role === 'Admin') return true;

   if (role !== 'Voter') return router.parseUrl('/admin');
  // if voter tries admin route
  router.navigate(['/voter']);
  return false;
};
