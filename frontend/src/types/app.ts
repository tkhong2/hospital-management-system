export interface HospitalApp {
  id: string
  code: string
  name: string
  description: string
  icon: string
  color: string
  path: string
  requiredPermission?: string
  badge?: string
}

export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  errors?: string[]
  traceId?: string
}

