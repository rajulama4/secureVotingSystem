import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login';
import { SignupComponent } from './pages/signup/signup';
import { AdminComponent } from './pages/admin/admin';
import { VoterComponent } from './pages/voter/voter';
import { ShellComponent } from './layout/shell/shell';

import { adminGuard } from './guards/admin-guard';
import { voterGuard } from './guards/voter-guard';

import { TotpComponent } from './pages/totp/totp';
import { ApiLogsComponent } from './pages/api-logs/api-logs';
import { CreateElectionComponent } from './pages/createelection/create-election';
import { ManageElectionsComponent } from './pages/manageelections/manage-elections';
import { VerificationComponent } from './pages/admin/verification/verification';
import { ChangePasswordComponent } from './pages//change-password/change-password';
import { VoterElectionsComponent } from './pages//voter-elections/voter-elections';
import { ManageCandidatesComponent } from './pages/manage-candidate/manage-candidates';
import { VoteCountStatusComponent } from './pages/vote-count-status/vote-count-status';
import { VoteCountDetailComponent } from './pages/vote-count-detail/vote-count-detail';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  { path: 'login', component: LoginComponent },
   { path: 'signup', component: SignupComponent },
  { path: 'totp', component: TotpComponent },
  { path: 'change-password', component: ChangePasswordComponent },

  {
    path: '',
    component: ShellComponent,
    children: [
      { path: 'admin', component: AdminComponent, canActivate: [adminGuard] },
      { path: 'admin/create-election', component: CreateElectionComponent, canActivate: [adminGuard] },
      { path: 'admin/manage-elections', component: ManageElectionsComponent, canActivate: [adminGuard] },
      { path: 'admin/api-logs', component: ApiLogsComponent, canActivate: [adminGuard] },
      { path: 'admin/manage-candidates', component: ManageCandidatesComponent, canActivate: [adminGuard] },
      { path: 'voter', component: VoterComponent, canActivate: [voterGuard] },
      { path: 'voter/elections', component: VoterElectionsComponent, canActivate: [voterGuard] },
      { path: 'admin/verification', component: VerificationComponent, canActivate: [adminGuard] },
      { path: 'admin/vote-count-status', component: VoteCountStatusComponent, canActivate: [adminGuard] },
      { path: 'admin/vote-count-status/:electionId', component: VoteCountDetailComponent, canActivate: [adminGuard] },
      
    ]
  },

  { path: '**', redirectTo: 'login' }
];