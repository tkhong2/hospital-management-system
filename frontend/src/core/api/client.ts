import axios, { type AxiosRequestConfig, type InternalAxiosRequestConfig } from 'axios'
import type { ApiResponse } from '@/core/types/api'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5101/api/v1',
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 30000
})

let isRefreshing = false
let failedQueue: Array<{
  resolve: (token: string) => void
  reject: (error: any) => void
}> = []

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token!)
    }
  })
  failedQueue = []
}

// Request Interceptor: Attach JWT Bearer Token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = localStorage.getItem('hospitalms_token')
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

// Response Interceptor: Handle 401 with Token Rotation & Request Queuing
apiClient.interceptors.response.use(
  (response) => response.data,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      // If the 401 is from login or refresh endpoint itself, don't retry -> logout
      if (
        originalRequest.url?.includes('/auth/login') ||
        originalRequest.url?.includes('/auth/refresh')
      ) {
        localStorage.removeItem('hospitalms_token')
        localStorage.removeItem('hospitalms_refresh_token')
        localStorage.removeItem('hospitalms_user')
        if (window.location.pathname !== '/login') {
          window.location.href = '/login'
        }
        return Promise.reject(error.response?.data || error)
      }

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then((newToken) => {
            originalRequest.headers.Authorization = `Bearer ${newToken}`
            return apiClient(originalRequest)
          })
          .catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      const currentAccessToken = localStorage.getItem('hospitalms_token')
      const currentRefreshToken = localStorage.getItem('hospitalms_refresh_token')

      if (!currentRefreshToken) {
        isRefreshing = false
        localStorage.removeItem('hospitalms_token')
        localStorage.removeItem('hospitalms_refresh_token')
        localStorage.removeItem('hospitalms_user')
        if (window.location.pathname !== '/login') {
          window.location.href = '/login'
        }
        return Promise.reject(error)
      }

      try {
        const refreshResponse = await axios.post(
          `${apiClient.defaults.baseURL}/auth/refresh`,
          {
            accessToken: currentAccessToken || '',
            refreshToken: currentRefreshToken
          },
          { headers: { 'Content-Type': 'application/json' } }
        )

        const responseData = refreshResponse.data
        if (responseData.success && responseData.data) {
          const { accessToken, refreshToken, user } = responseData.data
          localStorage.setItem('hospitalms_token', accessToken)
          localStorage.setItem('hospitalms_refresh_token', refreshToken)
          if (user) {
            localStorage.setItem('hospitalms_user', JSON.stringify(user))
          }

          apiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`
          originalRequest.headers.Authorization = `Bearer ${accessToken}`

          processQueue(null, accessToken)
          return apiClient(originalRequest)
        } else {
          throw new Error('Refresh token invalid')
        }
      } catch (refreshErr) {
        processQueue(refreshErr, null)
        localStorage.removeItem('hospitalms_token')
        localStorage.removeItem('hospitalms_refresh_token')
        localStorage.removeItem('hospitalms_user')
        if (window.location.pathname !== '/login') {
          window.location.href = '/login'
        }
        return Promise.reject(refreshErr)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error.response?.data || error)
  }
)

export async function get<T>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
  return apiClient.get(url, config) as unknown as Promise<ApiResponse<T>>
}

export async function post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
  return apiClient.post(url, data, config) as unknown as Promise<ApiResponse<T>>
}

export async function put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
  return apiClient.put(url, data, config) as unknown as Promise<ApiResponse<T>>
}

export async function del<T>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
  return apiClient.delete(url, config) as unknown as Promise<ApiResponse<T>>
}

export default apiClient
