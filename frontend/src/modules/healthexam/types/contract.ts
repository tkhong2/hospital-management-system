export interface HealthExamContract {
  id: string
  contractNumber: string
  companyName: string
  taxCode: string
  representativeName: string
  phoneNumber: string
  startDate: string
  endDate: string
  isMobileExam: boolean
  estimatedPatientCount: number
  totalContractAmount: number
  totalPaidAmount: number
  remainingDebt: number
  status: string
}

export interface ExamStation {
  id: string
  code: string
  name: string
  specialty: string
  doctorInCharge?: string
  waitingCount: number
  completedCount: number
  icon: string
}
