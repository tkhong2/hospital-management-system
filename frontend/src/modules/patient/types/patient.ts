export enum Gender {
  Unknown = 0,
  Male = 1,
  Female = 2,
  Other = 3
}

export enum BloodType {
  Unknown = 0,
  A_Positive = 1,
  A_Negative = 2,
  B_Positive = 3,
  B_Negative = 4,
  O_Positive = 5,
  O_Negative = 6,
  AB_Positive = 7,
  AB_Negative = 8
}

export interface Patient {
  id: string
  patientCode: string
  fullName: string
  dateOfBirth: string
  gender: Gender
  identityCardNumber?: string
  healthInsuranceCardNumber?: string
  phoneNumber?: string
  email?: string
  address?: string
  provinceCode?: string
  provinceName?: string
  districtCode?: string
  districtName?: string
  wardCode?: string
  wardName?: string
  bloodType?: BloodType
  allergies?: string
  medicalHistory?: string
  isMerged: boolean
  mergedToPatientId?: string
  createdAt: string
  updatedAt?: string
}

export interface PatientListItem {
  id: string
  patientCode: string
  fullName: string
  dateOfBirth: string
  gender: Gender
  identityCardNumber?: string
  healthInsuranceCardNumber?: string
  phoneNumber?: string
  address?: string
  provinceName?: string
  districtName?: string
  wardName?: string
  isMerged: boolean
  mergedToPatientId?: string
  createdAt: string
}

export interface DuplicateCandidate {
  id: string
  patientCode: string
  fullName: string
  dateOfBirth: string
  identityCardNumber?: string
  healthInsuranceCardNumber?: string
  phoneNumber?: string
  duplicateReasons: string[]
}

export interface PatientFilterParams {
  pageNumber?: number
  pageSize?: number
  search?: string
  gender?: Gender
  includeMerged?: boolean
  provinceCode?: string
}

export interface CreatePatientDto {
  fullName: string
  dateOfBirth: string
  gender: Gender
  identityCardNumber?: string
  healthInsuranceCardNumber?: string
  phoneNumber?: string
  email?: string
  address?: string
  provinceCode?: string
  provinceName?: string
  districtCode?: string
  districtName?: string
  wardCode?: string
  wardName?: string
  bloodType?: BloodType
  allergies?: string
  medicalHistory?: string
}

export interface UpdatePatientDto {
  fullName: string
  dateOfBirth: string
  gender: Gender
  identityCardNumber?: string
  healthInsuranceCardNumber?: string
  phoneNumber?: string
  email?: string
  address?: string
  provinceCode?: string
  provinceName?: string
  districtCode?: string
  districtName?: string
  wardCode?: string
  wardName?: string
  bloodType?: BloodType
  allergies?: string
  medicalHistory?: string
}

export interface MergePatientDto {
  sourcePatientId: string
  targetPatientId: string
  mergeReason: string
}

