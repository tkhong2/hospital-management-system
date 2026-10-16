import apiClient from '@/core/api/client'
import type { ApiResponse, PagedResult } from '@/core/types/api'
import type {
  Patient,
  PatientListItem,
  DuplicateCandidate,
  PatientFilterParams,
  CreatePatientDto,
  UpdatePatientDto,
  MergePatientDto
} from '../types/patient'

const BASE_URL = import.meta.env.VITE_PATIENT_API_URL || 'http://localhost:5102/api/v1/patients'

export const patientApi = {
  async getPatients(params?: PatientFilterParams): Promise<ApiResponse<PagedResult<PatientListItem>>> {
    return apiClient.get(`${BASE_URL}`, { params })
  },

  async getPatientById(id: string): Promise<ApiResponse<Patient>> {
    return apiClient.get(`${BASE_URL}/${id}`)
  },

  async createPatient(data: CreatePatientDto): Promise<ApiResponse<string>> {
    return apiClient.post(`${BASE_URL}`, data)
  },

  async updatePatient(id: string, data: UpdatePatientDto): Promise<ApiResponse<boolean>> {
    return apiClient.put(`${BASE_URL}/${id}`, data)
  },

  async deletePatient(id: string): Promise<ApiResponse<boolean>> {
    return apiClient.delete(`${BASE_URL}/${id}`)
  },

  async checkDuplicates(params: {
    identityCardNumber?: string
    healthInsuranceCardNumber?: string
    fullName?: string
    dateOfBirth?: string
    phoneNumber?: string
    excludePatientId?: string
  }): Promise<ApiResponse<DuplicateCandidate[]>> {
    return apiClient.get(`${BASE_URL}/check-duplicate`, { params })
  },

  async mergePatients(data: MergePatientDto): Promise<ApiResponse<boolean>> {
    return apiClient.post(`${BASE_URL}/merge`, data)
  }
}

