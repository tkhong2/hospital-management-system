import apiClient from '@/core/api/client'
import type { ApiResponse, PagedResult } from '@/core/types/api'
import type {
  MedicalService,
  Icd10,
  Department,
  Specialty,
  Unit,
  AdministrativeDivision,
  MedicalServiceFilterParams,
  Icd10FilterParams,
  CreateMedicalServiceDto,
  ImportMedicalServiceItem,
  ImportIcd10Item
} from '../types/catalog'

const BASE_URL = import.meta.env.VITE_CATALOG_API_URL || 'http://localhost:5103/api/v1'

export const catalogApi = {
  // Medical Services
  async getMedicalServices(params?: MedicalServiceFilterParams): Promise<ApiResponse<PagedResult<MedicalService>>> {
    return apiClient.get(`${BASE_URL}/medicalservices`, { params })
  },

  async createMedicalService(data: CreateMedicalServiceDto): Promise<ApiResponse<string>> {
    return apiClient.post(`${BASE_URL}/medicalservices`, data)
  },

  // ICD-10
  async getIcd10(params?: Icd10FilterParams): Promise<ApiResponse<PagedResult<Icd10>>> {
    return apiClient.get(`${BASE_URL}/icd10`, { params })
  },

  // Common Catalogs
  async getDepartments(): Promise<ApiResponse<Department[]>> {
    return apiClient.get(`${BASE_URL}/commoncatalogs/departments`)
  },

  async getSpecialties(): Promise<ApiResponse<Specialty[]>> {
    return apiClient.get(`${BASE_URL}/commoncatalogs/specialties`)
  },

  async getUnits(): Promise<ApiResponse<Unit[]>> {
    return apiClient.get(`${BASE_URL}/commoncatalogs/units`)
  },

  async getAdministrativeDivisions(parentCode?: string): Promise<ApiResponse<AdministrativeDivision[]>> {
    return apiClient.get(`${BASE_URL}/commoncatalogs/administrative-divisions`, {
      params: { parentCode }
    })
  },

  // Import
  async importMedicalServices(items: ImportMedicalServiceItem[]): Promise<ApiResponse<{ totalProcessed: number; successCount: number; failureCount: number }>> {
    return apiClient.post(`${BASE_URL}/import/medical-services`, { items })
  },

  async importIcd10(items: ImportIcd10Item[]): Promise<ApiResponse<{ totalProcessed: number; successCount: number; failureCount: number }>> {
    return apiClient.post(`${BASE_URL}/import/icd10`, { items })
  }
}

