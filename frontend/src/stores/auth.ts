import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserProfile, LoginRequest } from '@/types/auth'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('hospitalms_token'))
  const user = ref<UserProfile | null>(
    JSON.parse(localStorage.getItem('hospitalms_user') || 'null')
  )

  const isAuthenticated = computed(() => !!token.value)
  const accessibleApps = computed(() => user.value?.accessibleApps || [])

  function hasPermission(permission: string): boolean {
    if (!user.value) return false
    if (user.value.roles.includes('Admin') || user.value.roles.includes('SuperAdmin')) return true
    return user.value.permissions.includes(permission)
  }

  function canAccessApp(appCode: string): boolean {
    if (!user.value) return false
    if (user.value.roles.includes('Admin') || user.value.roles.includes('SuperAdmin')) return true
    return accessibleApps.value.includes(appCode)
  }

  async function login(credentials: LoginRequest): Promise<void> {
    // Simulated demo login / connected to IdentityService
    const mockUser: UserProfile = {
      id: '11111111-1111-1111-1111-111111111111',
      username: credentials.username,
      fullName: credentials.username === 'admin' ? 'Quản trị viên Hệ thống' : 'Bác sĩ CK I - Nguyễn Văn A',
      email: `${credentials.username}@hospital.local`,
      roles: credentials.username === 'admin' ? ['Admin'] : ['Doctor'],
      permissions: ['*'],
      accessibleApps: ['healthexam', 'outpatient', 'inpatient', 'pharmacy', 'patient', 'catalog', 'admin'],
      departmentName: 'Khoa Khám bệnh'
    }

    const mockToken = 'mock-jwt-token-hospitalms'
    token.value = mockToken
    user.value = mockUser

    localStorage.setItem('hospitalms_token', mockToken)
    localStorage.setItem('hospitalms_user', JSON.stringify(mockUser))
  }

  function logout(): void {
    token.value = null
    user.value = null
    localStorage.removeItem('hospitalms_token')
    localStorage.removeItem('hospitalms_user')
  }

  return {
    token,
    user,
    isAuthenticated,
    accessibleApps,
    hasPermission,
    canAccessApp,
    login,
    logout
  }
})

