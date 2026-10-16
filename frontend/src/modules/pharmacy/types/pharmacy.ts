export enum MedicineType {
  Medicine = 1,
  MedicalSupply = 2,
  Chemical = 3
}

export enum SpecialControlType {
  None = 0,
  Narcotic = 1,     // Thuốc gây nghiện
  Psychotropic = 2, // Thuốc hướng thần
  Precursor = 3,    // Tiền chất
  Toxic = 4,        // Thuốc độc
  Antibiotic = 5    // Kháng sinh ưu tiên quản lý
}

export enum WarehouseType {
  MainWarehouse = 1,
  SubWarehouse = 2,
  EmergencyCabinet = 3,
  InpatientWarehouse = 4
}

export enum DocumentType {
  Inbound = 1,
  Outbound = 2,
  Transfer = 3,
  Return = 4,
  Dispose = 5
}

export enum DocumentStatus {
  Draft = 1,
  Approved = 2,
  Cancelled = 3
}

export enum PrescriptionStatus {
  Pending = 1,
  Approved = 2,
  Dispensed = 3,
  Cancelled = 4
}

export interface Medicine {
  id: string
  code: string
  name: string
  registrationNumber?: string
  activeIngredient: string
  concentration: string
  unitName: string
  packageSpecification?: string
  routeOfAdministration?: string
  medicineGroup?: string
  medicineType: MedicineType
  specialControl: SpecialControlType
  inboundPrice: number
  salePrice: number
  insurancePrice?: number
  minStockLevel: number
  maxStockLevel: number
  isActive: boolean
}

export interface Warehouse {
  id: string
  code: string
  name: string
  warehouseType: WarehouseType
  warehouseTypeName: string
  departmentId?: string
  departmentName?: string
  location?: string
  managerName?: string
  isActive: boolean
}

export interface InventoryBatch {
  id: string
  warehouseId: string
  warehouseName: string
  medicineId: string
  medicineCode: string
  medicineName: string
  unitName: string
  batchNumber: string
  expiryDate: string
  manufactureDate?: string
  inboundPrice: number
  salePrice: number
  quantityOnHand: number
  quantityReserved: number
  availableQuantity: number
  daysUntilExpiry: number
  isNearExpiry: boolean
  isExpired: boolean
}

export interface RealtimeStock {
  medicineId: string
  medicineCode: string
  medicineName: string
  activeIngredient: string
  concentration: string
  unitName: string
  specialControl: SpecialControlType
  totalOnHand: number
  totalReserved: number
  totalAvailable: number
  minStockLevel: number
  isUnderMinStock: boolean
  batches: InventoryBatch[]
}

export interface StockAlert {
  alertType: 'Outdated' | 'NearExpiry' | 'UnderMinStock'
  severity: 'Critical' | 'Warning'
  medicineId: string
  medicineCode: string
  medicineName: string
  warehouseId: string
  warehouseName: string
  batchNumber?: string
  expiryDate?: string
  currentQuantity: number
  thresholdQuantity?: number
  message: string
}

export interface StockDocumentItem {
  medicineId: string
  medicineCode?: string
  medicineName?: string
  sourceBatchId?: string
  batchNumber: string
  expiryDate: string
  manufactureDate?: string
  quantity: number
  unitPrice: number
  totalPrice: number
  note?: string
}

export interface StockDocument {
  id: string
  documentNumber: string
  documentType: DocumentType
  documentTypeName: string
  sourceWarehouseId?: string
  sourceWarehouseName?: string
  targetWarehouseId?: string
  targetWarehouseName?: string
  supplierName?: string
  documentDate: string
  status: DocumentStatus
  statusName: string
  totalAmount: number
  note?: string
  createdBy?: string
  approvedBy?: string
  approvedAt?: string
  details: StockDocumentItem[]
}

export interface PrescriptionOrderDetail {
  id: string
  medicineId: string
  medicineCode: string
  medicineName: string
  unitName: string
  quantityRequested: number
  quantityDispensed: number
  unitPrice: number
  totalPrice: number
  usageInstructions?: string
  allocatedBatchId?: string
  allocatedBatchNumber?: string
  allocatedExpiryDate?: string
}

export interface PrescriptionOrder {
  id: string
  prescriptionCode: string
  sourceType: number
  sourceTypeName: string
  sourceReferenceId?: string
  patientId?: string
  patientCode: string
  patientName: string
  doctorName?: string
  diagnosis?: string
  prescribedAt: string
  warehouseId: string
  warehouseName: string
  status: PrescriptionStatus
  statusName: string
  totalAmount: number
  dispensedBy?: string
  dispensedAt?: string
  note?: string
  details: PrescriptionOrderDetail[]
}

export interface StockReportItem {
  medicineId: string
  medicineCode: string
  medicineName: string
  unitName: string
  openingBalance: number
  inboundQuantity: number
  outboundQuantity: number
  closingBalance: number
  unitPrice: number
  closingAmount: number
}

