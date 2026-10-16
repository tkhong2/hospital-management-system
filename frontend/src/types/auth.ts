export interface UserProfile {
  id: string
  username: string
  fullName: string
  email: string
  roles: string[]
  permissions: string[]
  accessibleApps: string[]
  departmentId?: string
  departmentName?: string
}

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserProfile
}

