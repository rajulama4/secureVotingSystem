import { Component } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TokenService } from '../../services/token';

// Material
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatListModule
  ],
  templateUrl: './shell.html',
  styleUrl: './shell.css'
})
export class ShellComponent {
  role: 'Admin' | 'Voter' | null = null;

  constructor(private tokenSvc: TokenService, private router: Router) {
    const token = this.tokenSvc.getToken(); // make sure TokenService has getToken()
    this.role = token ? (this.tokenSvc.getRole(token) as any) : null;
  }

  get isAdmin() {
    return this.role === 'Admin';
  }

  get isVoter() {
    return this.role === 'Voter';
  }

  logout() {
    this.tokenSvc.clear();
    this.router.navigate(['/login']);
  }
}