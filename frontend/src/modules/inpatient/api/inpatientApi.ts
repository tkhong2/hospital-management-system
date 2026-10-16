import axios from 'axios'
import type {
  BedMapSummary,
  InpatientAdmission,
  MedicalOrder,
  VitalSign,
  CareNote,
  AdvancePayment,
  BedStatus
} from '../types/inpatient'

const INPATIENT_API_BASE = 'http://localhost:5105/api/v1'

export const inpatientApi = {
  // Beds & Map
  async getBedMap(departmentCode?: string): Promise<BedMapSummary> {
    const params: Record<string, any> = {}
    if (departmentCode) params.departmentCode = departmentCode
    const res = await axios.get(`${INPATIENT_API_BASE}/beds/map`, { params })
    return res.data.data
  },

  async transferBed(data: {
    admissionId: string
    targetBedId: string
    reason: string
    transferredBy?: string
  }): Promise<boolean> {
    const res = await axios.post(`${INPATIENT_API_BASE}/beds/transfer`, data)
    return res.data.data
  },

  async updateBedStatus(bedId: string, newStatus: BedStatus, notes?: string): Promise<boolean> {
    const params: Record<string, any> = { newStatus }
    if (notes) params.notes = notes
    const res = await axios.put(`${INPATIENT_API_BASE}/beds/${bedId}/status`, null, { params })
    return res.data.data
  },

  // Admissions
  async createAdmission(data: {
    patientId: string
    patientCode: string
    patientName: string
    dateOfBirth?: string
    gender: string
    healthInsuranceNumber?: string
    relativeContact?: string
    departmentCode: string
    departmentName: string
    targetBedId: string
    admittingDoctorName?: string
    initialDiagnosis: string
    primaryIcd10Code: string
    primaryIcd10Name: string
    initialAdvancePayment?: number
  }): Promise<InpatientAdmission> {
    const res = await axios.post(`${INPATIENT_API_BASE}/admissions`, data)
    return res.data.data
  },

  async getActiveAdmissions(departmentCode?: string): Promise<InpatientAdmission[]> {
    const params: Record<string, any> = {}
    if (departmentCode) params.departmentCode = departmentCode
    const res = await axios.get(`${INPATIENT_API_BASE}/admissions/active`, { params })
    return res.data.data
  },

  async getAdmissionDetail(id: string): Promise<InpatientAdmission> {
    const res = await axios.get(`${INPATIENT_API_BASE}/admissions/${id}`)
    return res.data.data
  },

  async dischargeAdmission(id: string, data: {
    admissionId: string
    dischargeDiagnosis: string
    dischargeIcd10Code: string
    outcome: number
    dischargeSummaryNotes: string
    dischargeDoctorName: string
  }): Promise<boolean> {
    const res = await axios.post(`${INPATIENT_API_BASE}/admissions/${id}/discharge`, data)
    return res.data.data
  },

  // Clinical
  async createMedicalOrder(data: {
    admissionId: string
    doctorName: string
    diagnosis: string
    careLevel: number
    dietPlan?: string
    medicinesContent: string
    servicesContent?: string
    specialInstructions?: string
  }): Promise<MedicalOrder> {
    const res = await axios.post(`${INPATIENT_API_BASE}/clinical/orders`, data)
    return res.data.data
  },

  async recordVitalSign(data: {
    admissionId: string
    pulse: number
    temperature: number
    bloodPressureSystolic: number
    bloodPressureDiastolic: number
    breathingRate: number
    spO2: number
    weight?: number
    notes?: string
    recordedByNurse?: string
  }): Promise<VitalSign> {
    const res = await axios.post(`${INPATIENT_API_BASE}/clinical/vitals`, data)
    return res.data.data
  },

  async createCareNote(data: {
    admissionId: string
    clinicalEvolution: string
    nursingActions: string
    evaluation: string
    nurseName: string
  }): Promise<CareNote> {
    const res = await axios.post(`${INPATIENT_API_BASE}/clinical/care-notes`, data)
    return res.data.data
  },

  async recordAdvancePayment(data: {
    admissionId: string
    amount: number
    paymentMethod: string
    cashierName: string
    notes?: string
  }): Promise<AdvancePayment> {
    const res = await axios.post(`${INPATIENT_API_BASE}/clinical/advances`, data)
    return res.data.data
  }
}

