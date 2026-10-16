export enum ReceptionStatus {
  Waiting = 0,
  InConsultation = 1,
  WaitingForLab = 2,
  Completed = 3,
  Cancelled = 4
}

export enum PatientType {
  OutpatientInsurance = 0,
  ServiceFee = 1
}

export enum ServiceOrderStatus {
  Ordered = 0,
  SampleCollected = 1,
  Processing = 2,
  Completed = 3,
  Cancelled = 4
}

export enum PrescriptionStatus {
  Draft = 0,
  Prescribed = 1,
  Dispensed = 2,
  Cancelled = 3
}

export enum BillingStatus {
  Pending = 0,
  Paid = 1,
  PartiallyRefunded = 2,
  Cancelled = 3
}

export enum TreatmentDisposition {
  DischargedWithPrescription = 0,
  Hospitalize = 1,
  TransferHospital = 2,
  FollowUp = 3
}

export interface Reception {
  id: string
  queueNumber: number
  patientId: string
  patientCode: string
  patientName: string
  dateOfBirth?: string
  gender: string
  phoneNumber?: string
  healthInsuranceNumber?: string
  clinicRoomId: string
  clinicRoomName: string
  patientType: PatientType
  status: ReceptionStatus
  receptionTime: string
  reason?: string
  priority: number
  encounterId?: string
}

export interface Encounter {
  id: string
  receptionId: string
  patientId: string
  patientCode: string
  patientName: string
  doctorId?: string
  doctorName?: string
  clinicRoomName: string
  startTime: string
  endTime?: string
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
  disposition: TreatmentDisposition
  doctorNotes?: string
  followUpDate?: string
  serviceOrders: ServiceOrder[]
  prescriptions: Prescription[]
}

export interface ServiceOrder {
  id: string
  encounterId: string
  orderCode: string
  orderDate: string
  doctorName?: string
  status: ServiceOrderStatus
  totalAmount: number
  totalInsuranceAmount: number
  totalPatientAmount: number
  details: ServiceOrderDetail[]
}

export interface ServiceOrderDetail {
  id: string
  serviceId: string
  serviceCode: string
  serviceName: string
  serviceGroup: string
  quantity: number
  unitPrice: number
  insuranceCoverageRate: number
  insuranceCoveredAmount: number
  patientPayAmount: number
  roomToExecute?: string
  resultSummary?: string
  resultTime?: string
  isAbnormal: boolean
}

export interface Prescription {
  id: string
  encounterId: string
  prescriptionCode: string
  prescriptionDate: string
  doctorName?: string
  diagnosis: string
  daysCount: number
  advice?: string
  status: PrescriptionStatus
  totalAmount: number
  totalInsuranceAmount: number
  totalPatientAmount: number
  items: PrescriptionItem[]
}

export interface PrescriptionItem {
  id: string
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
  insuranceCoveredAmount: number
  patientPayAmount: number
}

export interface Billing {
  id: string
  encounterId: string
  patientId: string
  invoiceNumber: string
  invoiceDate: string
  totalAmount: number
  insuranceCoveredAmount: number
  patientPayAmount: number
  discountAmount: number
  amountPaid: number
  paymentMethod: string
  cashierName?: string
  status: BillingStatus
  items: BillingItem[]
}

export interface BillingItem {
  id: string
  itemType: string
  itemReferenceId: string
  itemCode: string
  itemName: string
  quantity: number
  unitPrice: number
  totalAmount: number
  insuranceCoveredAmount: number
  patientPayAmount: number
}

