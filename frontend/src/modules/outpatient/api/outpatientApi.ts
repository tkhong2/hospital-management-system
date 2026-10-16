import axios from 'axios'
import type {
  Reception,
  Encounter,
  ServiceOrder,
  Prescription,
  Billing,
  ReceptionStatus
} from '../types/outpatient'

const OUTPATIENT_API_BASE = 'http://localhost:5104/api/v1'

export const outpatientApi = {
  // Receptions
  async getQueue(clinicRoomId?: string, status?: ReceptionStatus): Promise<Reception[]> {
    const params: Record<string, any> = {}
    if (clinicRoomId) params.clinicRoomId = clinicRoomId
    if (status !== undefined) params.status = status
    const res = await axios.get(`${OUTPATIENT_API_BASE}/receptions/queue`, { params })
    return res.data.data
  },

  async createReception(data: {
    patientId: string
    patientCode: string
    patientName: string
    dateOfBirth?: string
    gender: string
    phoneNumber?: string
    address?: string
    healthInsuranceNumber?: string
    clinicRoomId: string
    clinicRoomName: string
    patientType: number
    reason?: string
    priority?: number
  }): Promise<Reception> {
    const res = await axios.post(`${OUTPATIENT_API_BASE}/receptions`, data)
    return res.data.data
  },

  // Encounters
  async startEncounter(data: {
    receptionId: string
    doctorId?: string
    doctorName?: string
  }): Promise<Encounter> {
    const res = await axios.post(`${OUTPATIENT_API_BASE}/encounters/start`, data)
    return res.data.data
  },

  async updateDiagnosis(id: string, data: {
    encounterId: string
    chiefComplaint: string
    historyOfPresentIllness?: string
    medicalHistory?: string
    physicalExam?: string
    pulse?: number
    bloodPressureSystolic?: number
    bloodPressureDiastolic?: number
    temperature?: number
    spO2?: number
    weight?: number
    height?: number
    primaryIcd10Code: string
    primaryIcd10Name: string
    secondaryIcd10Code?: string
    secondaryIcd10Name?: string
    clinicalDiagnosis?: string
    disposition: number
    doctorNotes?: string
    followUpDate?: string
  }): Promise<Encounter> {
    const res = await axios.put(`${OUTPATIENT_API_BASE}/encounters/${id}/diagnosis`, data)
    return res.data.data
  },

  async completeEncounter(id: string): Promise<boolean> {
    const res = await axios.post(`${OUTPATIENT_API_BASE}/encounters/${id}/complete`)
    return res.data.data
  },

  async getEncounter(id: string): Promise<Encounter> {
    const res = await axios.get(`${OUTPATIENT_API_BASE}/encounters/${id}`)
    return res.data.data
  },

  // Orders
  async createServiceOrder(data: {
    encounterId: string
    doctorName?: string
    items: Array<{
      serviceId: string
      serviceCode: string
      serviceName: string
      serviceGroup: string
      quantity: number
      unitPrice: number
      insuranceCoverageRate: number
      roomToExecute?: string
    }>
  }): Promise<ServiceOrder> {
    const res = await axios.post(`${OUTPATIENT_API_BASE}/orders/services`, data)
    return res.data.data
  },

  async createPrescription(data: {
    encounterId: string
    doctorName?: string
    diagnosis: string
    daysCount: number
    advice?: string
    items: Array<{
      medicineId: string
      medicineCode: string
      medicineName: string
      activeIngredient?: string
      dosageForm?: string
      quantity: number
      morningDose?: string
      noonDose?: string
      afternoonDose?: string
      nightDose?: string
      usageInstructions?: string
      unitPrice: number
      isInsuranceCovered: boolean
    }>
  }): Promise<Prescription> {
    const res = await axios.post(`${OUTPATIENT_API_BASE}/orders/prescriptions`, data)
    return res.data.data
  },

  async processBilling(data: {
    encounterId: string
    paymentMethod: string
    cashierName?: string
  }): Promise<Billing> {
    const res = await axios.post(`${OUTPATIENT_API_BASE}/orders/billings`, data)
    return res.data.data
  }
}

