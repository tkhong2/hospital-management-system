import axios, { type AxiosRequestConfig } from 'axios'
import type { ApiResponse } from '@/core/types/api'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api/v1',
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 30000
})

apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('hospitalms_token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

apiClient.interceptors.response.use(
  (response) => response.data,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('hospitalms_token')
      localStorage.removeItem('hospitalms_user')
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
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
