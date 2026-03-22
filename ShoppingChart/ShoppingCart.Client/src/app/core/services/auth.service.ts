import { HttpClient } from '@angular/common/http';
import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private isBrowser: boolean;
  private accessToken$ = new BehaviorSubject<string | null>(null);

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    if (this.isBrowser) {
      this.accessToken$.next(localStorage.getItem('accessToken'));
    }
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(API_ENDPOINTS.auth.login, request)
      .pipe(tap((res) => this.storeTokens(res)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(API_ENDPOINTS.auth.register, request)
      .pipe(tap((res) => this.storeTokens(res)));
  }

  logout(): void {
    if (this.isBrowser) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('userEmail');
      localStorage.removeItem('userId');
    }
    this.accessToken$.next(null);
  }

  getAccessToken(): string | null {
    return this.accessToken$.value;
  }

  isAuthenticated(): boolean {
    return !!this.accessToken$.value;
  }

  private storeTokens(response: AuthResponse): void {
    if (this.isBrowser) {
      localStorage.setItem('accessToken', response.accessToken);
      localStorage.setItem('userEmail', response.email);
      localStorage.setItem('userId', response.userId.toString());
      localStorage.setItem('refreshToken', response.refreshToken);
    }
    this.accessToken$.next(response.accessToken);
  }

  get currentUser(): { userId: number; email: string } | null {
    if (!this.isBrowser) {
      return null;
    }
    const token = localStorage.getItem('accessToken');
    const email = localStorage.getItem('userEmail');
    const userId = localStorage.getItem('userId');

    if (token && email && userId) {
      return { userId: Number(userId), email };
    }
    return null;
  }
}
