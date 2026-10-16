import { get, post } from '@/core/api/client'
import type { ApiResponse, PagedResult, PaginationParams } from '@/core/types/api'
import type { HealthExamContract } from '../types/contract'

export const contractApi = {
  getContracts(params?: PaginationParams): Promise<ApiResponse<PagedResult<HealthExamContract>>> {
    return get<PagedResult<HealthExamContract>>('/contracts', { params })
  },

  getContractById(id: string): Promise<ApiResponse<HealthExamContract>> {
    return get<HealthExamContract>(`/contracts/${id}`)
  },

  createContract(data: Partial<HealthExamContract>): Promise<ApiResponse<HealthExamContract>> {
    return post<HealthExamContract>('/contracts', data)
  }
}
