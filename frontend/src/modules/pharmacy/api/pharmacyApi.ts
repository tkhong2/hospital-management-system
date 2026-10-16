import apiClient from '@/core/api/client'
import type { ApiResponse, PagedResult } from '@/core/types/api'
import type {
  Medicine,
  Warehouse,
  RealtimeStock,
  StockAlert,
  StockDocument,
  StockDocumentItem,
  PrescriptionOrder,
  StockReportItem,
  DocumentType,
  DocumentStatus,
  SpecialControlType,
  MedicineType,
  PrescriptionStatus
} from '../types/pharmacy'

const BASE_URL = import.meta.env.VITE_PHARMACY_API_URL || 'http://localhost:5106/api/v1'

export const pharmacyApi = {
  // Medicines
  async getMedicines(params?: {
    pageNumber?: number
    pageSize?: number
    search?: string
    medicineType?: MedicineType
    specialControl?: SpecialControlType
    isActive?: boolean
  }): Promise<ApiResponse<PagedResult<Medicine>>> {
    return apiClient.get(`${BASE_URL}/medicines`, { params })
  },

  async createMedicine(data: any): Promise<ApiResponse<string>> {
    return apiClient.post(`${BASE_URL}/medicines`, data)
  },

  // Warehouses
  async getWarehouses(isActive = true): Promise<ApiResponse<Warehouse[]>> {
    return apiClient.get(`${BASE_URL}/warehouses`, { params: { isActive } })
  },

  // Inventory Realtime & Alerts
  async getRealtimeInventory(params?: {
    warehouseId?: string
    search?: string
    onlyUnderMinStock?: boolean
  }): Promise<ApiResponse<RealtimeStock[]>> {
    return apiClient.get(`${BASE_URL}/inventory/realtime`, { params })
  },

  async getStockAlerts(warehouseId?: string): Promise<ApiResponse<StockAlert[]>> {
    return apiClient.get(`${BASE_URL}/inventory/alerts`, { params: { warehouseId } })
  },

  // Stock Documents (Inbound, Outbound, Transfer)
  async getStockDocuments(params?: {
    pageNumber?: number
    pageSize?: number
    documentType?: DocumentType
    status?: DocumentStatus
    warehouseId?: string
    search?: string
  }): Promise<ApiResponse<PagedResult<StockDocument>>> {
    return apiClient.get(`${BASE_URL}/stockdocuments`, { params })
  },

  async createStockDocument(data: {
    documentType: DocumentType
    sourceWarehouseId?: string
    targetWarehouseId?: string
    supplierName?: string
    note?: string
    details: StockDocumentItem[]
  }): Promise<ApiResponse<string>> {
    return apiClient.post(`${BASE_URL}/stockdocuments`, data)
  },

  async approveStockDocument(id: string, approvedBy = 'Dược sĩ phụ trách'): Promise<ApiResponse<boolean>> {
    return apiClient.post(`${BASE_URL}/stockdocuments/${id}/approve`, null, { params: { approvedBy } })
  },

  // Prescriptions
  async getPrescriptionOrders(params?: {
    pageNumber?: number
    pageSize?: number
    status?: PrescriptionStatus
    warehouseId?: string
    search?: string
  }): Promise<ApiResponse<PagedResult<PrescriptionOrder>>> {
    return apiClient.get(`${BASE_URL}/prescriptions`, { params })
  },

  async dispensePrescription(id: string, warehouseId?: string, dispensedBy = 'Dược sĩ phụ trách'): Promise<ApiResponse<boolean>> {
    return apiClient.post(`${BASE_URL}/prescriptions/${id}/dispense`, null, {
      params: { warehouseId, dispensedBy }
    })
  },

  // Reports
  async getStockReport(params?: {
    warehouseId?: string
    fromDate?: string
    toDate?: string
    search?: string
  }): Promise<ApiResponse<StockReportItem[]>> {
    return apiClient.get(`${BASE_URL}/reports/stock-ledger`, { params })
  }
}

