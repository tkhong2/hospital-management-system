export enum ServiceType {
  Examination = 1,
  Laboratory = 2,
  Imaging = 3,
  FunctionalProbe = 4,
  Procedure = 5
}

export interface MedicalService {
  id: string
  code: string
  name: string
  serviceType: ServiceType
  serviceTypeName: string
  departmentId?: string
  departmentName?: string
  unitName?: string
  price: number
  insurancePrice?: number
  description?: string
  isActive: boolean
  createdAt: string
}

export interface Icd10 {
  id: string
  code: string
  nameVi: string
  nameEn?: string
  chapterCode?: string
  chapterName?: string
  isActive: boolean
}

export interface Department {
  id: string
  code: string
  name: string
  departmentType: string
  phone?: string
  location?: string
  isActive: boolean
}

export interface Specialty {
  id: string
  code: string
  name: string
  description?: string
  isActive: boolean
}

export interface Unit {
  id: string
  code: string
  name: string
  isActive: boolean
}

export interface AdministrativeDivision {
  id: string
  code: string
  name: string
  level: string
  parentCode?: string
}

export interface MedicalServiceFilterParams {
  pageNumber?: number
  pageSize?: number
  search?: string
  serviceType?: ServiceType
  departmentId?: string
  isActive?: boolean
}

export interface Icd10FilterParams {
  pageNumber?: number
  pageSize?: number
  search?: string
  chapterCode?: string
}

export interface CreateMedicalServiceDto {
  code: string
  name: string
  serviceType: ServiceType
  departmentId?: string
  unitName?: string
  price: number
  insurancePrice?: number
  description?: string
  isActive: boolean
}

export interface ImportMedicalServiceItem {
  code: string
  name: string
  serviceType: number
  departmentCode?: string
  unitName?: string
  price: number
  insurancePrice?: number
}

export interface ImportIcd10Item {
  code: string
  nameVi: string
  nameEn?: string
  chapterCode?: string
  chapterName?: string
}

