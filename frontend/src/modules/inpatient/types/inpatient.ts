export enum BedStatus {
  Empty = 0,
  Occupied = 1,
  Cleaning = 2,
  Maintenance = 3
}

export enum BedType {
  Standard = 0,
  Emergency = 1,
  RequestService = 2
}

export enum AdmissionStatus {
  Admitted = 0,
  Transferring = 1,
  Discharged = 2,
  Cancelled = 3
}

export enum DischargeOutcome {
  Cured = 0,
  Improved = 1,
  Unchanged = 2,
  Worsened = 3,
  Deceased = 4
}

export enum CareLevel {
  Level1 = 1,
  Level2 = 2,
  Level3 = 3
}

export interface HospitalBed {
  id: string
  roomId: string
  roomNumber: string
  roomName: string
  departmentName: string
  bedCode: string
  bedName: string
  bedType: BedType
  status: BedStatus
  currentAdmissionId?: string
  patientCode?: string
  patientName?: string
  initialDiagnosis?: string
  admissionTime?: string
  dailyPrice: number
  insuranceCoveredPrice: number
}

export interface DepartmentRoom {
  id: string
  departmentCode: string
  departmentName: string
  roomNumber: string
  roomName: string
  floor: number
  notes?: string
  beds: HospitalBed[]
}

export interface BedMapSummary {
  totalBeds: number
  emptyBeds: number
  occupiedBeds: number
  cleaningBeds: number
  maintenanceBeds: number
  occupancyRate: number
  rooms: DepartmentRoom[]
}

export interface InpatientAdmission {
  id: string
  admissionCode: string
  patientId: string
  patientCode: string
  patientName: string
  dateOfBirth?: string
  gender: string
  healthInsuranceNumber?: string
  relativeContact?: string
  admissionTime: string
  departmentCode: string
  departmentName: string
  currentBedId?: string
  currentBedCode?: string
  admittingDoctorName?: string
  initialDiagnosis: string
  primaryIcd10Code: string
  primaryIcd10Name: string
  status: AdmissionStatus
  totalAdvancePayment: number
  totalInpatientExpenses: number
  dischargeTime?: string
  dischargeDiagnosis?: string
  dischargeIcd10Code?: string
  outcome?: DischargeOutcome
  dischargeSummaryNotes?: string
  dischargeDoctorName?: string
  transferHistories: BedTransferHistory[]
  medicalOrders: MedicalOrder[]
  vitalSigns: VitalSign[]
  careNotes: CareNote[]
  advancePayments: AdvancePayment[]
}

export interface BedTransferHistory {
  id: string
  admissionId: string
  fromBedId?: string
  fromBedCode?: string
  toBedId: string
  toBedCode: string
  fromDepartmentName?: string
  toDepartmentName: string
  transferTime: string
  reason: string
  transferredBy?: string
}

export interface MedicalOrder {
  id: string
  admissionId: string
  orderDate: string
  doctorName: string
  diagnosis: string
  careLevel: CareLevel
  dietPlan?: string
  medicinesContent: string
  servicesContent?: string
  specialInstructions?: string
  isExecuted: boolean
  executedByNurse?: string
  executedTime?: string
}

export interface VitalSign {
  id: string
  admissionId: string
  recordTime: string
  pulse: number
  temperature: number
  bloodPressureSystolic: number
  bloodPressureDiastolic: number
  breathingRate: number
  spO2: number
  weight?: number
  notes?: string
  recordedByNurse?: string
}

export interface CareNote {
  id: string
  admissionId: string
  noteTime: string
  clinicalEvolution: string
  nursingActions: string
  evaluation: string
  nurseName: string
}

export interface AdvancePayment {
  id: string
  admissionId: string
  receiptNumber: string
  paymentDate: string
  amount: number
  paymentMethod: string
  cashierName: string
  notes?: string
}

