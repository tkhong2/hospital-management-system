<script setup lang="ts">
import { ref, onMounted } from 'vue'
import {
  Database,
  Stethoscope,
  BookOpen,
  Building2,
  MapPin,
  PlusCircle,
  FileUp,
  Search,
  CheckCircle,
  XCircle
} from 'lucide-vue-next'
import DataTable, { type ColumnDefinition } from '@/core/components/DataTable.vue'
import StatusBadge from '@/core/components/StatusBadge.vue'
import MedicalServiceModal from '../components/MedicalServiceModal.vue'
import ImportCatalogModal from '../components/ImportCatalogModal.vue'
import { catalogApi } from '../api/catalogApi'
import {
  ServiceType,
  type MedicalService,
  type Icd10,
  type Department,
  type Specialty,
  type Unit,
  type AdministrativeDivision
} from '../types/catalog'

const activeTab = ref<'services' | 'icd10' | 'departments' | 'adminUnits'>('services')

// --- TAB 1: MEDICAL SERVICES ---
const serviceColumns: ColumnDefinition[] = [
  { key: 'code', label: 'Mã Dịch Vụ', sortable: true, width: '130px' },
  { key: 'name', label: 'Tên Dịch Vụ Kỹ Thuật', sortable: true },
  { key: 'serviceTypeName', label: 'Nhóm Kỹ Thuật', width: '160px' },
  { key: 'departmentName', label: 'Khoa Thực Hiện', width: '160px' },
  { key: 'unitName', label: 'ĐVT', width: '80px', align: 'center' },
  { key: 'price', label: 'Giá Viện Phí', sortable: true, width: '130px', align: 'right' },
  { key: 'insurancePrice', label: 'Giá BHYT', sortable: true, width: '130px', align: 'right' },
  { key: 'status', label: 'Trạng Thái', width: '110px', align: 'center' }
]

const services = ref<MedicalService[]>([])
const servicesTotal = ref(0)
const servicesLoading = ref(false)
const servicesPage = ref(1)
const servicesPageSize = ref(10)
const servicesSearch = ref('')
const selectedServiceType = ref<ServiceType | undefined>(undefined)

const fetchServices = async () => {
  servicesLoading.value = true
  try {
    const res = await catalogApi.getMedicalServices({
      pageNumber: servicesPage.value,
      pageSize: servicesPageSize.value,
      search: servicesSearch.value || undefined,
      serviceType: selectedServiceType.value
    })
    if (res.success && res.data) {
      services.value = res.data.items
      servicesTotal.value = res.data.totalCount
    } else {
      services.value = []
      servicesTotal.value = 0
    }
  } catch (err) {
    console.error('Error fetching medical services:', err)
  } finally {
    servicesLoading.value = false
  }
}

// --- TAB 2: ICD-10 ---
const icd10Columns: ColumnDefinition[] = [
  { key: 'code', label: 'Mã ICD-10', sortable: true, width: '120px' },
  { key: 'nameVi', label: 'Tên Bệnh (Tiếng Việt)', sortable: true },
  { key: 'nameEn', label: 'Tên Tiếng Anh', sortable: false },
  { key: 'chapterCode', label: 'Chương', width: '100px', align: 'center' },
  { key: 'chapterName', label: 'Tên Chương Bệnh' }
]

const icd10Items = ref<Icd10[]>([])
const icd10Total = ref(0)
const icd10Loading = ref(false)
const icd10Page = ref(1)
const icd10PageSize = ref(10)
const icd10Search = ref('')

const fetchIcd10 = async () => {
  icd10Loading.value = true
  try {
    const res = await catalogApi.getIcd10({
      pageNumber: icd10Page.value,
      pageSize: icd10PageSize.value,
      search: icd10Search.value || undefined
    })
    if (res.success && res.data) {
      icd10Items.value = res.data.items
      icd10Total.value = res.data.totalCount
    } else {
      icd10Items.value = []
      icd10Total.value = 0
    }
  } catch (err) {
    console.error('Error fetching ICD10:', err)
  } finally {
    icd10Loading.value = false
  }
}

// --- TAB 3: DEPARTMENTS & SPECIALTIES ---
const departments = ref<Department[]>([])
const specialties = ref<Specialty[]>([])
const loadingDepSpecialties = ref(false)

const fetchDepartmentsAndSpecialties = async () => {
  loadingDepSpecialties.value = true
  try {
    const [depRes, specRes] = await Promise.all([
      catalogApi.getDepartments(),
      catalogApi.getSpecialties()
    ])
    if (depRes.success && depRes.data) departments.value = depRes.data
    if (specRes.success && specRes.data) specialties.value = specRes.data
  } catch (err) {
    console.error('Error fetching departments & specialties:', err)
  } finally {
    loadingDepSpecialties.value = false
  }
}

// --- TAB 4: UNITS & ADMINISTRATIVE DIVISIONS ---
const units = ref<Unit[]>([])
const administrativeDivisions = ref<AdministrativeDivision[]>([])
const loadingUnitsAdmin = ref(false)

const fetchUnitsAndAdmin = async () => {
  loadingUnitsAdmin.value = true
  try {
    const [uRes, aRes] = await Promise.all([
      catalogApi.getUnits(),
      catalogApi.getAdministrativeDivisions()
    ])
    if (uRes.success && uRes.data) units.value = uRes.data
    if (aRes.success && aRes.data) administrativeDivisions.value = aRes.data
  } catch (err) {
    console.error('Error fetching units & administrative divisions:', err)
  } finally {
    loadingUnitsAdmin.value = false
  }
}

// Modals
const showServiceModal = ref(false)
const showImportModal = ref(false)
const importType = ref<'services' | 'icd10'>('services')

const openImport = (type: 'services' | 'icd10') => {
  importType.value = type
  showImportModal.value = true
}

const formatCurrency = (val?: number) => {
  if (val === undefined || val === null) return '—'
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}

onMounted(() => {
  fetchServices()
  fetchDepartmentsAndSpecialties()
  fetchUnitsAndAdmin()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header banner -->
    <div class="flex flex-wrap items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-slate-700 to-slate-900 flex items-center justify-center text-white shadow-md">
          <Database class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Danh Mục Dùng Chung (Master Data Catalog)</h1>
          <p class="text-xs text-slate-500 mt-0.5">
            Dịch vụ kỹ thuật Thông tư 39, danh mục bệnh quốc tế ICD-10, cơ cấu khoa phòng, chuyên khoa và địa danh hành chính
          </p>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <button
          v-if="activeTab === 'services'"
          @click="showServiceModal = true"
          class="inline-flex items-center gap-2 px-4 py-2.5 bg-primary text-white text-xs font-bold rounded-xl hover:bg-primary-hover shadow-sm transition-all"
        >
          <PlusCircle class="w-4 h-4" />
          <span>Thêm Dịch Vụ Kỹ Thuật</span>
        </button>
        <button
          v-if="activeTab === 'services' || activeTab === 'icd10'"
          @click="openImport(activeTab === 'services' ? 'services' : 'icd10')"
          class="inline-flex items-center gap-2 px-3.5 py-2.5 bg-white border border-slate-200 text-slate-700 text-xs font-bold rounded-xl hover:bg-slate-50 shadow-xs transition-all"
        >
          <FileUp class="w-4 h-4 text-slate-500" />
          <span>Import Excel/JSON</span>
        </button>
      </div>
    </div>

    <!-- Navigation Tabs -->
    <div class="flex items-center gap-2 border-b border-slate-200 bg-white px-4 rounded-xl shadow-xs">
      <button
        @click="activeTab = 'services'; fetchServices()"
        class="inline-flex items-center gap-2 py-3.5 px-4 text-xs font-bold border-b-2 transition-colors"
        :class="activeTab === 'services' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:text-slate-800'"
      >
        <Stethoscope class="w-4 h-4" />
        <span>Dịch Vụ Kỹ Thuật & Giá</span>
      </button>

      <button
        @click="activeTab = 'icd10'; fetchIcd10()"
        class="inline-flex items-center gap-2 py-3.5 px-4 text-xs font-bold border-b-2 transition-colors"
        :class="activeTab === 'icd10' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:text-slate-800'"
      >
        <BookOpen class="w-4 h-4" />
        <span>Mã Bệnh ICD-10</span>
      </button>

      <button
        @click="activeTab = 'departments'; fetchDepartmentsAndSpecialties()"
        class="inline-flex items-center gap-2 py-3.5 px-4 text-xs font-bold border-b-2 transition-colors"
        :class="activeTab === 'departments' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:text-slate-800'"
      >
        <Building2 class="w-4 h-4" />
        <span>Khoa Phòng & Chuyên Khoa</span>
      </button>

      <button
        @click="activeTab = 'adminUnits'; fetchUnitsAndAdmin()"
        class="inline-flex items-center gap-2 py-3.5 px-4 text-xs font-bold border-b-2 transition-colors"
        :class="activeTab === 'adminUnits' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:text-slate-800'"
      >
        <MapPin class="w-4 h-4" />
        <span>Đơn Vị Tính & Địa Bàn Hành Chính</span>
      </button>
    </div>

    <!-- TAB 1: MEDICAL SERVICES TABLE -->
    <div v-if="activeTab === 'services'">
      <DataTable
        :columns="serviceColumns"
        :items="services"
        :loading="servicesLoading"
        :total-count="servicesTotal"
        :page="servicesPage"
        :page-size="servicesPageSize"
        search-placeholder="Tìm kiếm mã hoặc tên dịch vụ kỹ thuật..."
        export-file-name="Danh_muc_dich_vu_ky_thuat"
        @update:search="(q) => { servicesSearch = q; servicesPage = 1; fetchServices() }"
        @update:page="(p) => { servicesPage = p; fetchServices() }"
        @update:pageSize="(s) => { servicesPageSize = s; servicesPage = 1; fetchServices() }"
      >
        <template #filters>
          <select
            v-model="selectedServiceType"
            @change="servicesPage = 1; fetchServices()"
            class="px-3 py-2 text-xs bg-white border border-slate-200 rounded-xl focus:ring-1 focus:ring-primary text-slate-700 shadow-xs"
          >
            <option :value="undefined">Tất cả nhóm kỹ thuật</option>
            <option :value="ServiceType.Examination">Khám bệnh</option>
            <option :value="ServiceType.Laboratory">Xét nghiệm</option>
            <option :value="ServiceType.Imaging">Chẩn đoán hình ảnh</option>
            <option :value="ServiceType.FunctionalProbe">Thăm dò chức năng</option>
            <option :value="ServiceType.Procedure">Thủ thuật</option>
          </select>
        </template>

        <template #cell-code="{ value }">
          <span class="font-mono font-bold text-primary">{{ value }}</span>
        </template>

        <template #cell-name="{ item }">
          <div>
            <span class="font-semibold text-slate-900">{{ item.name }}</span>
            <div v-if="item.description" class="text-[11px] text-slate-400 font-normal truncate max-w-sm">
              {{ item.description }}
            </div>
          </div>
        </template>

        <template #cell-serviceTypeName="{ value }">
          <span class="px-2 py-0.5 rounded text-[11px] font-semibold bg-slate-100 text-slate-700 border border-slate-200">
            {{ value }}
          </span>
        </template>

        <template #cell-price="{ value }">
          <span class="font-semibold text-slate-900">{{ formatCurrency(value) }}</span>
        </template>

        <template #cell-insurancePrice="{ value }">
          <span v-if="value" class="font-semibold text-emerald-700">{{ formatCurrency(value) }}</span>
          <span v-else class="text-slate-300 italic">—</span>
        </template>

        <template #cell-status="{ item }">
          <StatusBadge
            :status="item.isActive ? 'Đang áp dụng' : 'Tạm ngưng'"
            :variant="item.isActive ? 'success' : 'default'"
          />
        </template>
      </DataTable>
    </div>

    <!-- TAB 2: ICD-10 TABLE -->
    <div v-if="activeTab === 'icd10'">
      <DataTable
        :columns="icd10Columns"
        :items="icd10Items"
        :loading="icd10Loading"
        :total-count="icd10Total"
        :page="icd10Page"
        :page-size="icd10PageSize"
        search-placeholder="Tìm kiếm mã hoặc tên bệnh ICD-10..."
        export-file-name="Danh_muc_ICD10"
        @update:search="(q) => { icd10Search = q; icd10Page = 1; fetchIcd10() }"
        @update:page="(p) => { icd10Page = p; fetchIcd10() }"
        @update:pageSize="(s) => { icd10PageSize = s; icd10Page = 1; fetchIcd10() }"
      >
        <template #cell-code="{ value }">
          <span class="font-mono font-bold text-indigo-600 bg-indigo-50 px-2 py-0.5 rounded">{{ value }}</span>
        </template>

        <template #cell-nameVi="{ item }">
          <span class="font-semibold text-slate-800">{{ item.nameVi }}</span>
        </template>

        <template #cell-nameEn="{ value }">
          <span class="text-xs text-slate-500 italic">{{ value || '—' }}</span>
        </template>

        <template #cell-chapterCode="{ value }">
          <span class="font-mono text-xs font-bold text-slate-600">{{ value || '—' }}</span>
        </template>
      </DataTable>
    </div>

    <!-- TAB 3: DEPARTMENTS & SPECIALTIES -->
    <div v-if="activeTab === 'departments'" class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Departments Card -->
      <div class="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2">
            <Building2 class="w-5 h-5 text-primary" />
            <h3 class="font-bold text-slate-800 text-sm">Danh Mục Khoa / Phòng Chức Năng</h3>
          </div>
          <span class="text-xs text-slate-500">{{ departments.length }} đơn vị</span>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead>
              <tr class="border-b border-slate-100 text-slate-500 font-semibold uppercase">
                <th class="py-2.5 px-3">Mã</th>
                <th class="py-2.5 px-3">Tên Khoa / Phòng</th>
                <th class="py-2.5 px-3">Loại khoa</th>
                <th class="py-2.5 px-3">Vị trí</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 text-slate-700">
              <tr v-for="d in departments" :key="d.id" class="hover:bg-slate-50">
                <td class="py-2.5 px-3 font-mono font-bold text-primary">{{ d.code }}</td>
                <td class="py-2.5 px-3 font-semibold text-slate-800">{{ d.name }}</td>
                <td class="py-2.5 px-3 text-slate-500">{{ d.departmentType }}</td>
                <td class="py-2.5 px-3 text-slate-500">{{ d.location || '—' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Specialties Card -->
      <div class="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2">
            <Stethoscope class="w-5 h-5 text-indigo-600" />
            <h3 class="font-bold text-slate-800 text-sm">Danh Mục Chuyên Khoa Y Tế</h3>
          </div>
          <span class="text-xs text-slate-500">{{ specialties.length }} chuyên khoa</span>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead>
              <tr class="border-b border-slate-100 text-slate-500 font-semibold uppercase">
                <th class="py-2.5 px-3">Mã CK</th>
                <th class="py-2.5 px-3">Tên Chuyên Khoa</th>
                <th class="py-2.5 px-3">Mô tả</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 text-slate-700">
              <tr v-for="s in specialties" :key="s.id" class="hover:bg-slate-50">
                <td class="py-2.5 px-3 font-mono font-bold text-indigo-600">{{ s.code }}</td>
                <td class="py-2.5 px-3 font-semibold text-slate-800">{{ s.name }}</td>
                <td class="py-2.5 px-3 text-slate-500">{{ s.description || '—' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- TAB 4: UNITS & ADMINISTRATIVE DIVISIONS -->
    <div v-if="activeTab === 'adminUnits'" class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Units Card -->
      <div class="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2">
            <Database class="w-5 h-5 text-cyan-600" />
            <h3 class="font-bold text-slate-800 text-sm">Đơn Vị Tính Dịch Vụ & Vật Tư</h3>
          </div>
          <span class="text-xs text-slate-500">{{ units.length }} đơn vị tính</span>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead>
              <tr class="border-b border-slate-100 text-slate-500 font-semibold uppercase">
                <th class="py-2.5 px-3">Mã ĐVT</th>
                <th class="py-2.5 px-3">Tên Đơn Vị Tính</th>
                <th class="py-2.5 px-3 text-center">Trạng Thái</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 text-slate-700">
              <tr v-for="u in units" :key="u.id" class="hover:bg-slate-50">
                <td class="py-2.5 px-3 font-mono font-bold text-cyan-700">{{ u.code }}</td>
                <td class="py-2.5 px-3 font-semibold text-slate-800">{{ u.name }}</td>
                <td class="py-2.5 px-3 text-center">
                  <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-emerald-100 text-emerald-800">Hoạt động</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Administrative Divisions Card -->
      <div class="bg-white rounded-2xl border border-slate-200 p-5 shadow-sm space-y-4">
        <div class="flex items-center justify-between border-b border-slate-100 pb-3">
          <div class="flex items-center gap-2">
            <MapPin class="w-5 h-5 text-rose-600" />
            <h3 class="font-bold text-slate-800 text-sm">Địa Bàn Hành Chính Quốc Gia (3 Cấp)</h3>
          </div>
          <span class="text-xs text-slate-500">{{ administrativeDivisions.length }} địa bàn mẫu</span>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-left text-xs">
            <thead>
              <tr class="border-b border-slate-100 text-slate-500 font-semibold uppercase">
                <th class="py-2.5 px-3">Mã địa bàn</th>
                <th class="py-2.5 px-3">Tên đơn vị</th>
                <th class="py-2.5 px-3">Cấp hành chính</th>
                <th class="py-2.5 px-3">Mã cấp trên</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100 text-slate-700">
              <tr v-for="a in administrativeDivisions" :key="a.id" class="hover:bg-slate-50">
                <td class="py-2.5 px-3 font-mono font-bold text-rose-700">{{ a.code }}</td>
                <td class="py-2.5 px-3 font-semibold text-slate-800">{{ a.name }}</td>
                <td class="py-2.5 px-3">
                  <span class="px-2 py-0.5 rounded text-[10px] font-semibold bg-slate-100 text-slate-700">
                    {{ a.level }}
                  </span>
                </td>
                <td class="py-2.5 px-3 font-mono text-slate-400">{{ a.parentCode || '—' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <MedicalServiceModal
      v-model="showServiceModal"
      @saved="fetchServices"
    />

    <ImportCatalogModal
      v-model="showImportModal"
      :target-type="importType"
      @imported="importType === 'services' ? fetchServices() : fetchIcd10()"
    />
  </div>
</template>
