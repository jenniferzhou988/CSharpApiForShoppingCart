export interface RegisterRequest {
  firstName: string;
  middleName?: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  fullName?: string;
  orgId: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: number;
  email: string;
  accessToken: string;
  refreshToken: string;
  expires: string;
}

export interface AuthState {
  userId: number;
  email: string;
  accessToken: string;
  refreshToken: string;
  expires: string;
  roles: string[];
}
