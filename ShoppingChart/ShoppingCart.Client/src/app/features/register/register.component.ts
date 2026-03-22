import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../core/services/auth.service';
import { RegisterRequest } from '../../core/models/auth.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  registerForm: FormGroup;
  hidePassword = true;
  hideConfirmPassword = true;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.registerForm = this.fb.group(
      {
        firstName: ['', [Validators.required, Validators.maxLength(100)]],
        middleName: ['', [Validators.maxLength(100)]],
        lastName: ['', [Validators.required, Validators.maxLength(100)]],
        email: ['', [Validators.required, Validators.email]],
        password: [
          '',
          [Validators.required, Validators.minLength(8), Validators.maxLength(128)],
        ],
        confirmPassword: ['', [Validators.required]],
      },
      { validators: this.passwordsMatchValidator }
    );
  }

  /** Custom validator: confirmPassword must match password */
  private passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordsMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading = true;

    const formValue = this.registerForm.value;

    const request: RegisterRequest = {
      firstName: formValue.firstName,
      middleName: formValue.middleName || undefined,
      lastName: formValue.lastName,
      email: formValue.email,
      password: formValue.password,
      confirmPassword: formValue.confirmPassword,
      fullName: `${formValue.firstName} ${formValue.lastName}`.trim(),
      orgId: 0,
    };

    this.authService.register(request).subscribe({
      next: () => {
        this.loading = false;
        this.snackBar.open('Registration successful!', 'Close', { duration: 3000 });
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading = false;
        if (err.status === 409) {
          this.snackBar.open('Email is already registered.', 'Close', { duration: 5000 });
        } else if (err.error?.Error) {
          this.snackBar.open(err.error.Error, 'Close', { duration: 5000 });
        } else {
          this.snackBar.open('Registration failed. Please try again.', 'Close', { duration: 5000 });
        }
      },
    });
  }
}
