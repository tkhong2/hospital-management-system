export interface UserProfile {
  id: string
  username: string
  fullName: string
  email: string
  phoneNumber?: string
  departmentId?: string
  departmentName?: string
  roles: string[]
  permissions: string[]
  accessibleApps: string[]
}

export interface ApplicationItem {
  id: string
  code: string
  name: string
  description: string
  icon: string
  color: string
  path: string
  displayOrder: number
  permissions: string[]
}

export interface UserApplicationsResponse {
  userId: string
  username: string
  fullName: string
  roles: string[]
  permissions: string[]
  applications: ApplicationItem[]
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
