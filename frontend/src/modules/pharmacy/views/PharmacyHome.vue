<script setup lang="ts">
import { ref, onMounted } from 'vue'
import {
  Pill,
  Layers,
  AlertTriangle,
  FileText,
  ClipboardList,
  BarChart3,
  PlusCircle,
  CheckCircle,
  Clock,
  Warehouse as WarehouseIcon,
  Search,
  CheckCircle2,
  XCircle,
  FileCheck
} from 'lucide-vue-next'
import DataTable, { type ColumnDefinition } from '@/core/components/DataTable.vue'
import StatusBadge from '@/core/components/StatusBadge.vue'
import CreateMedicineModal from '../components/CreateMedicineModal.vue'
import CreateStockDocumentModal from '../components/CreateStockDocumentModal.vue'
import DispensePrescriptionModal from '../components/DispensePrescriptionModal.vue'
import { pharmacyApi } from '../api/pharmacyApi'
import {
  MedicineType,
  SpecialControlType,
  DocumentType,
  DocumentStatus,
  PrescriptionStatus,
  type Medicine,
  type Warehouse,
  type RealtimeStock,
  type StockAlert,
  type StockDocument,
  type PrescriptionOrder,
  type StockReportItem
} from '../types/pharmacy'

const activeTab = ref<'medicines' | 'inventory' | 'documents' | 'prescriptions' | 'reports'>('inventory')

// Common data
const warehouses = ref<Warehouse[]>([])
const selectedWarehouseId = ref<string>('')

// --- TAB 1: MEDICINES ---
const medicineColumns: ColumnDefinition[] = [
  { key: 'code', label: 'Mã Thuốc', sortable: true, width: '130px' },
  { key: 'name', label: 'Tên Thuốc / Biệt Dược', sortable: true },
  { key: 'activeIngredient', label: 'Hoạt Chất - Hàm Lượng', sortable: true },
  { key: 'unitName', label: 'ĐVT', width: '80px', align: 'center' },
  { key: 'specialControl', label: 'Kiểm Soát Đặc Biệt', width: '170px' },
  { key: 'salePrice', label: 'Giá Viện Phí', sortable: true, width: '130px', align: 'right' },
  { key: 'minStockLevel', label: 'Tồn Tối Thiểu', width: '120px', align: 'center' }
]

const medicines = ref<Medicine[]>([])
const medicinesTotal = ref(0)
const medicinesLoading = ref(false)
const medicinesPage = ref(1)
const medicinesPageSize = ref(10)
const medicinesSearch = ref('')
const selectedSpecialControl = ref<SpecialControlType | undefined>(undefined)

const fetchMedicines = async () => {
  medicinesLoading.value = true
  try {
    const res = await pharmacyApi.getMedicines({
      pageNumber: medicinesPage.value,
      pageSize: medicinesPageSize.value,
      search: medicinesSearch.value || undefined,
      specialControl: selectedSpecialControl.value
    })
    if (res.success && res.data) {
      medicines.value = res.data.items
      medicinesTotal.value = res.data.totalCount
    }
  } catch (err) {
    console.error('Error fetching medicines:', err)
  } finally {
    medicinesLoading.value = false
  }
}

// --- TAB 2: REALTIME INVENTORY & ALERTS ---
const realtimeStock = ref<RealtimeStock[]>([])
const stockAlerts = ref<StockAlert[]>([])
const inventoryLoading = ref(false)
const inventorySearch = ref('')
const onlyUnderMinStock = ref(false)

const fetchInventory = async () => {
  inventoryLoading.value = true
  try {
    const [stockRes, alertsRes] = await Promise.all([
      pharmacyApi.getRealtimeInventory({
        warehouseId: selectedWarehouseId.value || undefined,
        search: inventorySearch.value || undefined,
        onlyUnderMinStock: onlyUnderMinStock.value
      }),
      pharmacyApi.getStockAlerts(selectedWarehouseId.value || undefined)
    ])

    if (stockRes.success && stockRes.data) realtimeStock.value = stockRes.data
    if (alertsRes.success && alertsRes.data) stockAlerts.value = alertsRes.data
  } catch (err) {
    console.error('Error fetching inventory:', err)
  } finally {
    inventoryLoading.value = false
  }
}

// --- TAB 3: STOCK DOCUMENTS ---
const documentColumns: ColumnDefinition[] = [
  { key: 'documentNumber', label: 'Số Phiếu', sortable: true, width: '140px' },
  { key: 'documentTypeName', label: 'Loại Phiếu', width: '160px' },
  { key: 'sourceWarehouseName', label: 'Kho Xuất', width: '180px' },
  { key: 'targetWarehouseName', label: 'Kho Nhập', width: '180px' },
  { key: 'documentDate', label: 'Ngày Lập', sortable: true, width: '120px' },
  { key: 'totalAmount', label: 'Tổng Tiền', sortable: true, width: '140px', align: 'right' },
  { key: 'status', label: 'Trạng Thái', width: '130px', align: 'center' }
]

const documents = ref<StockDocument[]>([])
const documentsTotal = ref(0)
const documentsLoading = ref(false)
const documentsPage = ref(1)
const documentsPageSize = ref(10)
const selectedDocType = ref<DocumentType | undefined>(undefined)

const fetchDocuments = async () => {
  documentsLoading.value = true
  try {
    const res = await pharmacyApi.getStockDocuments({
      pageNumber: documentsPage.value,
      pageSize: documentsPageSize.value,
      documentType: selectedDocType.value,
      warehouseId: selectedWarehouseId.value || undefined
    })
    if (res.success && res.data) {
      documents.value = res.data.items
      documentsTotal.value = res.data.totalCount
    }
  } catch (err) {
    console.error('Error fetching documents:', err)
  } finally {
    documentsLoading.value = false
  }
}

const handleApproveDocument = async (doc: StockDocument) => {
  if (confirm(`Xác nhận duyệt ${doc.documentTypeName} [${doc.documentNumber}]? Hệ thống sẽ cập nhật số dư kho và ghi sổ thẻ kho.`)) {
    try {
      const res = await pharmacyApi.approveStockDocument(doc.id)
      if (res.success) {
        alert('Duyệt phiếu kho thành công!')
        fetchDocuments()
        fetchInventory()
      } else {
        alert(res.errors?.[0] || 'Lỗi khi duyệt phiếu')
      }
    } catch (err: any) {
      alert(err.message || 'Lỗi khi duyệt phiếu')
    }
  }
}

// --- TAB 4: PRESCRIPTION ORDERS ---
const prescriptionColumns: ColumnDefinition[] = [
  { key: 'prescriptionCode', label: 'Mã Đơn Thuốc', sortable: true, width: '160px' },
  { key: 'patientInfo', label: 'Bệnh Nhân', sortable: true },
  { key: 'sourceTypeName', label: 'Nguồn Đơn', width: '150px' },
  { key: 'doctorName', label: 'Bác Sĩ Kê Đơn', width: '160px' },
  { key: 'prescribedAt', label: 'Thời Gian Kê', sortable: true, width: '140px' },
  { key: 'totalAmount', label: 'Tổng Tiền', sortable: true, width: '130px', align: 'right' },
  { key: 'status', label: 'Trạng Thái', width: '130px', align: 'center' }
]

const prescriptions = ref<PrescriptionOrder[]>([])
const prescriptionsTotal = ref(0)
const prescriptionsLoading = ref(false)
const prescriptionsPage = ref(1)
const prescriptionsPageSize = ref(10)

const fetchPrescriptions = async () => {
  prescriptionsLoading.value = true
  try {
    const res = await pharmacyApi.getPrescriptionOrders({
      pageNumber: prescriptionsPage.value,
      pageSize: prescriptionsPageSize.value,
      warehouseId: selectedWarehouseId.value || undefined
    })
    if (res.success && res.data) {
      prescriptions.value = res.data.items
      prescriptionsTotal.value = res.data.totalCount
    }
  } catch (err) {
    console.error('Error fetching prescriptions:', err)
  } finally {
    prescriptionsLoading.value = false
  }
}

// --- TAB 5: REPORTS ---
const reportColumns: ColumnDefinition[] = [
  { key: 'medicineCode', label: 'Mã Thuốc', sortable: true, width: '120px' },
  { key: 'medicineName', label: 'Tên Thuốc / Vật Tư', sortable: true },
  { key: 'unitName', label: 'ĐVT', width: '80px', align: 'center' },
  { key: 'openingBalance', label: 'Tồn Đầu Kỳ', width: '120px', align: 'right' },
  { key: 'inboundQuantity', label: 'Nhập Trong Kỳ', width: '120px', align: 'right' },
  { key: 'outboundQuantity', label: 'Xuất Trong Kỳ', width: '120px', align: 'right' },
  { key: 'closingBalance', label: 'Tồn Cuối Kỳ', width: '120px', align: 'right' },
  { key: 'closingAmount', label: 'Giá Trị Tồn Cuối', width: '150px', align: 'right' }
]

const reportItems = ref<StockReportItem[]>([])
const reportLoading = ref(false)

const fetchReports = async () => {
  reportLoading.value = true
  try {
    const res = await pharmacyApi.getStockReport({
      warehouseId: selectedWarehouseId.value || undefined
    })
    if (res.success && res.data) {
      reportItems.value = res.data
    }
  } catch (err) {
    console.error('Error fetching reports:', err)
  } finally {
    reportLoading.value = false
  }
}

// Modals
const showCreateMedicineModal = ref(false)
const showCreateDocModal = ref(false)
const showDispenseModal = ref(false)
const selectedPrescriptionToDispense = ref<PrescriptionOrder | null>(null)

const openDispenseModal = (order: PrescriptionOrder) => {
  selectedPrescriptionToDispense.value = order
  showDispenseModal.value = true
}

const formatCurrency = (val: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}

onMounted(async () => {
  try {
    const res = await pharmacyApi.getWarehouses()
    if (res.success && res.data) {
      warehouses.value = res.data
    }
  } catch (e) {
    console.error(e)
  }
  fetchInventory()
  fetchMedicines()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header banner -->
    <div class="flex flex-wrap items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center text-white shadow-md">
          <Pill class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Quản Lý Kho Dược & Cấp Phát Thuốc (Pharmacy)</h1>
          <p class="text-xs text-slate-500 mt-0.5">
            Nguyên tắc xuất FEFO, quản lý lô - hạn dùng, kiểm soát thuốc gây nghiện/hướng thần, thẻ kho và đồng bộ cấp phát
          </p>
        </div>
      </div>

      <div class="flex items-center gap-3">
        <!-- Warehouse Selector -->
        <div class="flex items-center gap-2 bg-slate-50 px-3 py-1.5 rounded-xl border border-slate-200">
          <WarehouseIcon class="w-4 h-4 text-slate-500" />
          <select
            v-model="selectedWarehouseId"
            @change="fetchInventory(); fetchDocuments(); fetchPrescriptions(); fetchReports()"
            class="text-xs font-semibold text-slate-700 bg-transparent focus:outline-none cursor-pointer"
          >
            <option value="">Toàn bộ các kho</option>
            <option v-for="w in warehouses" :key="w.id" :value="w.id">
              {{ w.name }}
            </option>
          </select>
        </div>

        <button
          v-if="activeTab === 'medicines'"
          @click="showCreateMedicineModal = true"
          class="inline-flex items-center gap-2 px-4 py-2 bg-primary text-white text-xs font-bold rounded-xl hover:bg-primary-hover shadow-sm transition-all"
        >
          <PlusCircle class="w-4 h-4" />
          <span>Thêm Thuốc Mới</span>
        </button>

        <button
          v-if="activeTab === 'documents'"
          @click="showCreateDocModal = true"
          class="inline-flex items-center gap-2 px-4 py-2 bg-primary text-white text-xs font-bold rounded-xl hover:bg-primary-hover shadow-sm transition-all"
        >
          <PlusCircle class="w-4 h-4" />
          <span>Lập Phiếu Kho</span>
        </button>
      </div>
    </div>

    <!-- Navigation Tabs -->
    <div class="flex items-center gap-2 border-b border-slate-200 bg-white px-4 rounded-xl shadow-xs">
      <button
        @click="activeTab = 'inventory'; fetchInventory()"
        class="inline-flex items-center gap-2 py-3.5 px-4 text-xs font-bold border-b-2 transition-colors"
        :class="activeTab === 'inventory' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:text-slate-800'"
      >
        <Layers class="w-4 h-4" />
        <span>Tồn Kho Realtime & Cảnh Báo</span>
        <span
          v-if="stockAlerts.length > 0"
          class="ml-1 px-1.5 py-0.5 rounded-full text-[10px] font-bold bg-rose-500 text-white"
        >
          {{ stockAlerts.length }}
        </span>
      </button>

      <button
        @click="activeTab = 'medicines'; fetchMedicines()"
        class="inline-flex items-center gap-2 py-3.5 px-4 text-xs font-bold border-b-2 transition-colors"
        :class="activeTab === 'medicines' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:text-slate-800'"
      >
        <Pill class="w-4 h-4" />
        <span>Danh Mục Thuốc & Vật Tư</span>
      </button>

      <button
        @click="activeTab = 'documents'; fetchDocuments()"
        class="inline-flex items-center gap-2 py-3.5 px-4 text-xs font-bold border-b-2 transition-colors"
        :class="activeTab === 'documents' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:text-slate-800'"
      >
        <FileText class="w-4 h-4" />
        <span>Phiếu Kho & Lịch Sử</span>
      </button>

      <button
        @click="activeTab = 'prescriptions'; fetchPrescriptions()"
        class="inline-flex items-center gap-2 py-3.5 px-4 text-xs font-bold border-b-2 transition-colors"
        :class="activeTab === 'prescriptions' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:text-slate-800'"
      >
        <ClipboardList class="w-4 h-4" />
        <span>Duyệt & Cấp Phát Đơn Thuốc</span>
      </button>

      <button
        @click="activeTab = 'reports'; fetchReports()"
        class="inline-flex items-center gap-2 py-3.5 px-4 text-xs font-bold border-b-2 transition-colors"
        :class="activeTab === 'reports' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:text-slate-800'"
      >
        <BarChart3 class="w-4 h-4" />
        <span>Báo Cáo Nhập - Xuất - Tồn</span>
      </button>
    </div>

    <!-- TAB 1: REALTIME INVENTORY & ALERTS -->
    <div v-if="activeTab === 'inventory'" class="space-y-5">
      <!-- Stock Alerts Banner -->
      <div v-if="stockAlerts.length > 0" class="p-4 rounded-2xl bg-amber-50/80 border border-amber-200">
        <div class="flex items-center gap-2 text-amber-800 font-bold text-xs uppercase mb-2">
          <AlertTriangle class="w-4 h-4 text-amber-600" />
          <span>Cảnh Báo Dược Lâm Sàng & Quản Trị Tồn Kho ({{ stockAlerts.length }})</span>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2.5 max-h-48 overflow-y-auto pr-1">
          <div
            v-for="(a, idx) in stockAlerts"
            :key="idx"
            class="p-2.5 rounded-xl border text-xs flex items-start gap-2 shadow-xs"
            :class="a.severity === 'Critical' ? 'bg-rose-50 border-rose-200 text-rose-900' : 'bg-white border-amber-200 text-amber-900'"
          >
            <span
              class="w-2 h-2 rounded-full mt-1.5 shrink-0"
              :class="a.severity === 'Critical' ? 'bg-rose-600 animate-pulse' : 'bg-amber-500'"
            />
            <div class="flex-1">
              <span class="font-bold">[{{ a.medicineCode }}] {{ a.medicineName }}</span>
              <p class="text-[11px] mt-0.5 opacity-90">{{ a.message }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Realtime Stock Table -->
      <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden p-4 space-y-4">
        <div class="flex flex-wrap items-center justify-between gap-3">
          <div class="relative w-72">
            <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              v-model="inventorySearch"
              type="text"
              placeholder="Tìm theo tên thuốc, số lô..."
              @input="fetchInventory"
              class="w-full pl-10 pr-4 py-2 text-xs bg-white border border-slate-200 rounded-xl focus:ring-1 focus:ring-primary text-slate-700"
            />
          </div>

          <label class="inline-flex items-center gap-2 text-xs text-slate-600 cursor-pointer">
            <input
              v-model="onlyUnderMinStock"
              type="checkbox"
              @change="fetchInventory"
              class="rounded border-slate-300 text-primary"
            />
            <span>Chỉ xem thuốc dưới định mức tối thiểu</span>
          </label>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs border-collapse">
            <thead>
              <tr class="border-b border-slate-100 bg-slate-50 font-semibold text-slate-600 uppercase">
                <th class="py-3 px-3">Mã Thuốc</th>
                <th class="py-3 px-3">Tên Thuốc - Hoạt Chất</th>
                <th class="py-3 px-3 text-center">ĐVT</th>
                <th class="py-3 px-3 text-right">Tồn Khả Dụng</th>
                <th class="py-3 px-3 text-right">Giữ Chỗ</th>
                <th class="py-3 px-3 text-right">Tổng Tồn Vật Lý</th>
                <th class="py-3 px-3 text-center">Tồn Tối Thiểu</th>
                <th class="py-3 px-3">Chi Tiết Từng Lô & Hạn Dùng (FEFO)</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 text-slate-700">
              <tr v-for="item in realtimeStock" :key="item.medicineId" class="hover:bg-slate-50/60">
                <td class="py-3 px-3 font-mono font-bold text-primary">{{ item.medicineCode }}</td>
                <td class="py-3 px-3">
                  <span class="font-bold text-slate-900">{{ item.medicineName }}</span>
                  <div class="text-[11px] text-slate-400">
                    {{ item.activeIngredient }} ({{ item.concentration }})
                  </div>
                </td>
                <td class="py-3 px-3 text-center font-medium">{{ item.unitName }}</td>
                <td class="py-3 px-3 text-right font-bold text-emerald-700">{{ item.totalAvailable.toLocaleString() }}</td>
                <td class="py-3 px-3 text-right text-slate-400">{{ item.totalReserved.toLocaleString() }}</td>
                <td class="py-3 px-3 text-right font-bold text-slate-900">{{ item.totalOnHand.toLocaleString() }}</td>
                <td class="py-3 px-3 text-center">
                  <span
                    class="px-2 py-0.5 rounded text-[10px] font-bold"
                    :class="item.isUnderMinStock ? 'bg-rose-100 text-rose-700' : 'bg-slate-100 text-slate-600'"
                  >
                    {{ item.minStockLevel.toLocaleString() }}
                  </span>
                </td>
                <td class="py-3 px-3">
                  <div class="flex flex-wrap gap-1.5">
                    <span
                      v-for="b in item.batches"
                      :key="b.id"
                      class="inline-flex items-center gap-1 px-2 py-1 rounded-lg text-[11px] font-mono border"
                      :class="b.isExpired ? 'bg-rose-50 border-rose-200 text-rose-700' : b.isNearExpiry ? 'bg-amber-50 border-amber-200 text-amber-800' : 'bg-slate-50 border-slate-200 text-slate-700'"
                    >
                      <strong>{{ b.batchNumber }}</strong>: {{ b.quantityOnHand }} {{ item.unitName }}
                      <span class="text-[10px] opacity-75">({{ b.expiryDate.slice(0, 10) }})</span>
                    </span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- TAB 2: MEDICINES TABLE -->
    <div v-if="activeTab === 'medicines'">
      <DataTable
        :columns="medicineColumns"
        :items="medicines"
        :loading="medicinesLoading"
        :total-count="medicinesTotal"
        :page="medicinesPage"
        :page-size="medicinesPageSize"
        search-placeholder="Tìm kiếm theo mã, tên thuốc, hoạt chất..."
        export-file-name="Danh_muc_thuoc_vat_tu"
        @update:search="(q) => { medicinesSearch = q; medicinesPage = 1; fetchMedicines() }"
        @update:page="(p) => { medicinesPage = p; fetchMedicines() }"
        @update:pageSize="(s) => { medicinesPageSize = s; medicinesPage = 1; fetchMedicines() }"
      >
        <template #filters>
          <select
            v-model="selectedSpecialControl"
            @change="medicinesPage = 1; fetchMedicines()"
            class="px-3 py-2 text-xs bg-white border border-slate-200 rounded-xl text-slate-700 shadow-xs"
          >
            <option :value="undefined">Tất cả nhóm kiểm soát</option>
            <option :value="SpecialControlType.None">Thuốc thông thường</option>
            <option :value="SpecialControlType.Narcotic">Thuốc gây nghiện</option>
            <option :value="SpecialControlType.Psychotropic">Thuốc hướng thần</option>
            <option :value="SpecialControlType.Antibiotic">Kháng sinh</option>
          </select>
        </template>

        <template #cell-code="{ value }">
          <span class="font-mono font-bold text-primary">{{ value }}</span>
        </template>

        <template #cell-name="{ item }">
          <div>
            <span class="font-bold text-slate-800">{{ item.name }}</span>
            <div v-if="item.registrationNumber" class="text-[11px] text-slate-400 font-mono">
              SĐK: {{ item.registrationNumber }}
            </div>
          </div>
        </template>

        <template #cell-activeIngredient="{ item }">
          <span class="font-semibold text-slate-700">{{ item.activeIngredient }}</span>
          <span class="text-slate-400 ml-1">({{ item.concentration }})</span>
        </template>

        <template #cell-specialControl="{ value }">
          <span
            v-if="value === SpecialControlType.Narcotic"
            class="px-2 py-0.5 rounded text-[11px] font-bold bg-rose-100 text-rose-800 border border-rose-200"
          >
            GÂY NGHIỆN
          </span>
          <span
            v-else-if="value === SpecialControlType.Psychotropic"
            class="px-2 py-0.5 rounded text-[11px] font-bold bg-amber-100 text-amber-800 border border-amber-200"
          >
            HƯỚNG THẦN
          </span>
          <span
            v-else-if="value === SpecialControlType.Antibiotic"
            class="px-2 py-0.5 rounded text-[11px] font-bold bg-blue-100 text-blue-800 border border-blue-200"
          >
            KHÁNG SINH
          </span>
          <span v-else class="text-slate-400 text-xs">Bình thường</span>
        </template>

        <template #cell-salePrice="{ value }">
          <span class="font-semibold text-slate-800">{{ formatCurrency(value) }}</span>
        </template>

        <template #cell-minStockLevel="{ value }">
          <span class="font-semibold text-slate-600">{{ value.toLocaleString() }}</span>
        </template>
      </DataTable>
    </div>

    <!-- TAB 3: STOCK DOCUMENTS TABLE -->
    <div v-if="activeTab === 'documents'">
      <DataTable
        :columns="documentColumns"
        :items="documents"
        :loading="documentsLoading"
        :total-count="documentsTotal"
        :page="documentsPage"
        :page-size="documentsPageSize"
        export-file-name="Danh_sach_phieu_kho"
        @update:page="(p) => { documentsPage = p; fetchDocuments() }"
        @update:pageSize="(s) => { documentsPageSize = s; documentsPage = 1; fetchDocuments() }"
      >
        <template #filters>
          <select
            v-model="selectedDocType"
            @change="documentsPage = 1; fetchDocuments()"
            class="px-3 py-2 text-xs bg-white border border-slate-200 rounded-xl text-slate-700 shadow-xs"
          >
            <option :value="undefined">Tất cả loại phiếu</option>
            <option :value="DocumentType.Inbound">Phiếu nhập kho</option>
            <option :value="DocumentType.Transfer">Phiếu chuyển kho</option>
            <option :value="DocumentType.Outbound">Phiếu xuất kho</option>
            <option :value="DocumentType.Dispose">Phiếu xuất hủy</option>
          </select>
        </template>

        <template #cell-documentNumber="{ value }">
          <span class="font-mono font-bold text-primary">{{ value }}</span>
        </template>

        <template #cell-documentDate="{ value }">
          <span class="text-xs text-slate-600">{{ value?.slice(0, 10) }}</span>
        </template>

        <template #cell-totalAmount="{ value }">
          <span class="font-bold text-slate-900">{{ formatCurrency(value) }}</span>
        </template>

        <template #cell-status="{ item }">
          <StatusBadge
            :status="item.statusName"
            :variant="item.status === DocumentStatus.Approved ? 'success' : item.status === DocumentStatus.Cancelled ? 'danger' : 'warning'"
          />
        </template>

        <template #actions="{ item }">
          <button
            v-if="item.status === DocumentStatus.Draft"
            @click="handleApproveDocument(item)"
            class="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-bold text-white bg-emerald-600 hover:bg-emerald-700 rounded-lg transition-colors shadow-xs"
          >
            <FileCheck class="w-3.5 h-3.5" />
            <span>Duyệt</span>
          </button>
        </template>
      </DataTable>
    </div>

    <!-- TAB 4: PRESCRIPTION ORDERS TABLE -->
    <div v-if="activeTab === 'prescriptions'">
      <DataTable
        :columns="prescriptionColumns"
        :items="prescriptions"
        :loading="prescriptionsLoading"
        :total-count="prescriptionsTotal"
        :page="prescriptionsPage"
        :page-size="prescriptionsPageSize"
        export-file-name="Danh_sach_don_thuoc"
        @update:page="(p) => { prescriptionsPage = p; fetchPrescriptions() }"
        @update:pageSize="(s) => { prescriptionsPageSize = s; prescriptionsPage = 1; fetchPrescriptions() }"
      >
        <template #cell-prescriptionCode="{ value }">
          <span class="font-mono font-bold text-primary">{{ value }}</span>
        </template>

        <template #cell-patientInfo="{ item }">
          <div>
            <span class="font-bold text-slate-800 uppercase">{{ item.patientName }}</span>
            <div class="text-[11px] text-slate-400 font-mono">Mã BN: {{ item.patientCode }}</div>
          </div>
        </template>

        <template #cell-prescribedAt="{ value }">
          <span class="text-xs text-slate-600">{{ value?.slice(0, 16).replace('T', ' ') }}</span>
        </template>

        <template #cell-totalAmount="{ value }">
          <span class="font-bold text-emerald-700">{{ formatCurrency(value) }}</span>
        </template>

        <template #cell-status="{ item }">
          <StatusBadge
            :status="item.statusName"
            :variant="item.status === PrescriptionStatus.Dispensed ? 'success' : 'warning'"
          />
        </template>

        <template #actions="{ item }">
          <button
            @click="openDispenseModal(item)"
            class="inline-flex items-center gap-1 px-3 py-1 text-xs font-bold rounded-lg transition-colors shadow-xs"
            :class="item.status === PrescriptionStatus.Dispensed ? 'bg-slate-100 text-slate-600 hover:bg-slate-200' : 'bg-primary text-white hover:bg-primary-hover'"
          >
            <Pill class="w-3.5 h-3.5" />
            <span>{{ item.status === PrescriptionStatus.Dispensed ? 'Chi tiết' : 'Cấp phát FEFO' }}</span>
          </button>
        </template>
      </DataTable>
    </div>

    <!-- TAB 5: REPORTS TABLE -->
    <div v-if="activeTab === 'reports'" class="space-y-4">
      <DataTable
        :columns="reportColumns"
        :items="reportItems"
        :loading="reportLoading"
        :total-count="reportItems.length"
        export-file-name="Bao_cao_nhap_xuat_ton"
      >
        <template #cell-medicineCode="{ value }">
          <span class="font-mono font-bold text-primary">{{ value }}</span>
        </template>

        <template #cell-openingBalance="{ value }">
          <span class="font-semibold text-slate-700">{{ value.toLocaleString() }}</span>
        </template>

        <template #cell-inboundQuantity="{ value }">
          <span class="font-semibold text-blue-700">+{{ value.toLocaleString() }}</span>
        </template>

        <template #cell-outboundQuantity="{ value }">
          <span class="font-semibold text-rose-700">-{{ value.toLocaleString() }}</span>
        </template>

        <template #cell-closingBalance="{ value }">
          <span class="font-bold text-emerald-700">{{ value.toLocaleString() }}</span>
        </template>

        <template #cell-closingAmount="{ value }">
          <span class="font-bold text-slate-900">{{ formatCurrency(value) }}</span>
        </template>
      </DataTable>
    </div>

    <!-- Modals -->
    <CreateMedicineModal
      v-model="showCreateMedicineModal"
      @saved="fetchMedicines"
    />

    <CreateStockDocumentModal
      v-model="showCreateDocModal"
      :warehouses="warehouses"
      :medicines="medicines"
      @saved="fetchDocuments"
    />

    <DispensePrescriptionModal
      v-model="showDispenseModal"
      :prescription="selectedPrescriptionToDispense"
      @dispensed="fetchPrescriptions(); fetchInventory()"
    />
  </div>
</template>
