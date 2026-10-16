import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserProfile, LoginRequest, LoginResponse, ApplicationItem, UserApplicationsResponse } from '@/types/auth'
import { get, post } from '@/core/api/client'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('hospitalms_token'))
  const refreshToken = ref<string | null>(localStorage.getItem('hospitalms_refresh_token'))
  const user = ref<UserProfile | null>(
    JSON.parse(localStorage.getItem('hospitalms_user') || 'null')
  )
  const applications = ref<ApplicationItem[]>(
    JSON.parse(localStorage.getItem('hospitalms_applications') || '[]')
  )

  const isAuthenticated = computed(() => !!token.value)
  const accessibleApps = computed(() => {
    if (applications.value.length > 0) {
      return applications.value.map(a => a.code.toLowerCase())
    }
    return user.value?.accessibleApps?.map(a => a.toLowerCase()) || []
  })

  function hasPermission(permission: string): boolean {
    if (!user.value) return false
    const roles = user.value.roles || []
    if (roles.includes('Admin') || roles.includes('SuperAdmin')) return true

    const userPerms = user.value.permissions || []
    if (userPerms.includes('*')) return true

    return userPerms.some(p => p.toLowerCase() === permission.toLowerCase())
  }

  function canAccessApp(appCode: string): boolean {
    if (!user.value) return false
    const roles = user.value.roles || []
    if (roles.includes('Admin') || roles.includes('SuperAdmin')) return true

    const code = appCode.toLowerCase()
    return accessibleApps.value.some(a => a.toLowerCase() === code)
  }

  async function login(credentials: LoginRequest): Promise<void> {
    try {
      const response = await post<LoginResponse>('/auth/login', credentials)
      if (response.success && response.data) {
        const { accessToken, refreshToken: newRefreshToken, user: profile } = response.data

        token.value = accessToken
        refreshToken.value = newRefreshToken
        user.value = profile

        localStorage.setItem('hospitalms_token', accessToken)
        localStorage.setItem('hospitalms_refresh_token', newRefreshToken)
        localStorage.setItem('hospitalms_user', JSON.stringify(profile))

        // Fetch user's accessible applications from Identity Service
        await fetchMyApplications()
        return
      }
      throw new Error(response.errors?.join(', ') || 'Đăng nhập thất bại')
    } catch (err: any) {
      // If backend is not running, provide demo fallback for frontend standalone preview
      if (err.code === 'ERR_NETWORK' || err.message?.includes('Network Error')) {
        console.warn('Backend IdentityService unreachable. Falling back to local offline mode.')
        const mockProfile: UserProfile = {
          id: '11111111-1111-1111-1111-111111111111',
          username: credentials.username,
          fullName: credentials.username === 'admin' ? 'Quản trị viên Hệ thống' : 'Bác sĩ CK I - Nguyễn Văn A',
          email: `${credentials.username}@hospital.vn`,
          roles: credentials.username === 'admin' ? ['Admin'] : ['Doctor'],
          permissions: ['*'],
          accessibleApps: ['healthexam', 'outpatient', 'inpatient', 'pharmacy', 'patient', 'catalog', 'admin'],
          departmentName: 'Khoa Khám bệnh'
        }

        const mockToken = 'mock-jwt-token-hospitalms'
        token.value = mockToken
        user.value = mockProfile

        localStorage.setItem('hospitalms_token', mockToken)
        localStorage.setItem('hospitalms_user', JSON.stringify(mockProfile))
        return
      }

      const msg = err.errors?.join(', ') || err.message || 'Đăng nhập không thành công'
      throw new Error(msg)
    }
  }

  async function fetchMyApplications(): Promise<void> {
    try {
      const response = await get<UserApplicationsResponse>('/auth/me/applications')
      if (response.success && response.data) {
        applications.value = response.data.applications
        localStorage.setItem('hospitalms_applications', JSON.stringify(response.data.applications))

        // Update permissions list if provided
        if (user.value && response.data.permissions) {
          user.value.permissions = response.data.permissions
          localStorage.setItem('hospitalms_user', JSON.stringify(user.value))
        }
      }
    } catch (e) {
      console.warn('Could not fetch /auth/me/applications, using cached/profile values:', e)
    }
  }

  async function logout(): Promise<void> {
    try {
      if (refreshToken.value) {
        await post('/auth/logout', { refreshToken: refreshToken.value })
      }
    } catch (e) {
      console.warn('Logout API call failed:', e)
    } finally {
      token.value = null
      refreshToken.value = null
      user.value = null
      applications.value = []
      localStorage.removeItem('hospitalms_token')
      localStorage.removeItem('hospitalms_refresh_token')
      localStorage.removeItem('hospitalms_user')
      localStorage.removeItem('hospitalms_applications')
    }
  }

  return {
    token,
    refreshToken,
    user,
    applications,
    isAuthenticated,
    accessibleApps,
    hasPermission,
    canAccessApp,
    login,
    logout,
    fetchMyApplications
  }
})
