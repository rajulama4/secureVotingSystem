import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs/operators';

import { AuthService } from '../../services/auth';

import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';

import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatNativeDateModule

  ],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class SignupComponent {
  loading = false;
  error = '';
  success = '';
  registered = false;

  selectedFile: File | null = null;
  fileError = '';

  generated: { userId: string; password: string } | null = null;

  form = {
    fullName: '',
    email: '',
    phoneNumber: '',
    addressLine1: '',
    addressLine2: '',
    city: '',
    stateCode: '',
    zipCode: '',
    jurisdictionId: null as number | null,
    dateOfBirth: null as Date | null,
    idDocumentType: '',
    idDocumentNumber: '',
    idDocumentState: ''
  };

  idTypes = ['DriverLicense', 'StateID', 'Passport'];

  constructor(
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  register() {
    this.error = '';
    this.success = '';
    this.generated = null;
    this.registered = false;
    this.fileError = '';

    if (
      !this.form.fullName?.trim() ||
      !this.form.email?.trim() ||
      !this.form.phoneNumber?.trim() ||
      !this.form.addressLine1?.trim() ||
      !this.form.city?.trim() ||
      !this.form.stateCode?.trim() ||
      !this.form.zipCode?.trim() ||
      !this.form.idDocumentType?.trim() ||
      !this.form.idDocumentNumber?.trim()
    ) {
      this.error = 'Please fill in all required fields.';
      return;
    }

    if (!this.selectedFile) {
      this.fileError = 'ID picture is required.';
      return;
    }

    const formData = new FormData();
    formData.append('FullName', this.form.fullName.trim());
    formData.append('Email', this.form.email.trim());
    formData.append('PhoneNumber', this.form.phoneNumber.trim());
    formData.append('AddressLine1', this.form.addressLine1.trim());
    formData.append('AddressLine2', this.form.addressLine2?.trim() || '');
    formData.append('City', this.form.city.trim());
    formData.append('StateCode', this.form.stateCode.trim());
    formData.append('ZipCode', this.form.zipCode.trim());
    formData.append(
      'JurisdictionId',
      this.form.jurisdictionId !== null && this.form.jurisdictionId !== undefined
        ? this.form.jurisdictionId.toString()
        : ''
    );

  
    if (this.form.jurisdictionId !== null && this.form.jurisdictionId !== undefined) {
      formData.append('JurisdictionId', this.form.jurisdictionId.toString());
    }

    if (this.form.dateOfBirth) {
      const dob = this.form.dateOfBirth;
      const yyyy = dob.getFullYear();
      const mm = String(dob.getMonth() + 1).padStart(2, '0');
      const dd = String(dob.getDate()).padStart(2, '0');
      formData.append('DOB', `${yyyy}-${mm}-${dd}`);
    }

    formData.append('IdDocumentType', this.form.idDocumentType.trim());
    formData.append('IdDocumentNumber', this.form.idDocumentNumber.trim());
    formData.append('IdDocumentState', this.form.idDocumentState?.trim() || '');
    formData.append('IdPicture', this.selectedFile);

    this.loading = true;

    this.auth.registerVoter(formData)
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (res: any) => {
          console.log('Register response:', res);

          this.success = res?.message || 'Registration successful.';
          this.generated = {
            userId: res?.userId || '',
            password: res?.password || ''
          };
          this.registered = true;

          this.resetFormAfterSuccess();
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Register error:', err);
          this.error = err?.error?.message || 'Registration failed.';
          this.cdr.detectChanges();
        }
      });
  }

  onFileSelected(event: Event) {
    this.fileError = '';

    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      this.selectedFile = null;
      this.fileError = 'ID picture is required.';
      return;
    }

    const file = input.files[0];
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];

    if (!allowedTypes.includes(file.type)) {
      this.selectedFile = null;
      this.fileError = 'Only JPG, PNG, and WEBP files are allowed.';
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.selectedFile = null;
      this.fileError = 'Max file size is 5MB.';
      return;
    }

    this.selectedFile = file;
  }

  private resetFormAfterSuccess(): void {
    this.form = {
      fullName: '',
      email: '',
      phoneNumber: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      stateCode: '',
      zipCode: '',
      jurisdictionId: null,
      dateOfBirth: null,
      idDocumentType: '',
      idDocumentNumber: '',
      idDocumentState: ''
    };

    this.selectedFile = null;
    this.fileError = '';
  }
}