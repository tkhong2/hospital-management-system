<script setup lang="ts">
import { ref, onMounted } from 'vue'
import {
  Users,
  UserPlus,
  Edit,
  Trash2,
  GitMerge,
  ShieldCheck,
  CheckCircle,
  AlertCircle
} from 'lucide-vue-next'
import DataTable, { type ColumnDefinition } from '@/core/components/DataTable.vue'
import StatusBadge from '@/core/components/StatusBadge.vue'
import PatientFormModal from '../components/PatientFormModal.vue'
import PatientMergeModal from '../components/PatientMergeModal.vue'
import { patientApi } from '../api/patientApi'
import { Gender, type Patient, type PatientListItem } from '../types/patient'

const columns: ColumnDefinition[] = [
  { key: 'patientCode', label: 'Mã BN', sortable: true, width: '130px' },
  { key: 'fullName', label: 'Họ và tên', sortable: true },
  { key: 'dateOfBirth', label: 'Ngày sinh', sortable: true, width: '130px' },
  { key: 'gender', label: 'Giới tính', sortable: false, width: '90px', align: 'center' },
  { key: 'identityCardNumber', label: 'Số CCCD', sortable: false, width: '140px' },
  { key: 'healthInsuranceCardNumber', label: 'Mã BHYT', sortable: false, width: '140px' },
  { key: 'phoneNumber', label: 'Số điện thoại', sortable: false, width: '120px' },
  { key: 'address', label: 'Địa chỉ', sortable: false },
  { key: 'status', label: 'Trạng thái', sortable: false, width: '120px', align: 'center' }
]

const patients = ref<PatientListItem[]>([])
const totalCount = ref(0)
const loading = ref(false)
const page = ref(1)
const pageSize = ref(10)
const searchQuery = ref('')
const selectedGender = ref<Gender | undefined>(undefined)
const includeMerged = ref(false)

// Modals
const showFormModal = ref(false)
const patientToEdit = ref<Patient | null>(null)

const showMergeModal = ref(false)
const targetPatientToMerge = ref<PatientListItem | null>(null)

const fetchPatients = async () => {
  loading.value = true
  try {
    const res = await patientApi.getPatients({
      pageNumber: page.value,
      pageSize: pageSize.value,
      search: searchQuery.value || undefined,
      gender: selectedGender.value,
      includeMerged: includeMerged.value
    })

    if (res.success && res.data) {
      patients.value = res.data.items
      totalCount.value = res.data.totalCount
    } else {
      patients.value = []
      totalCount.value = 0
    }
  } catch (err) {
    console.error('Error fetching patients:', err)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchPatients()
})

const handleSearch = (query: string) => {
  searchQuery.value = query
  page.value = 1
  fetchPatients()
}

const handlePageChange = (newPage: number) => {
  page.value = newPage
  fetchPatients()
}

const handlePageSizeChange = (newSize: number) => {
  pageSize.value = newSize
  page.value = 1
  fetchPatients()
}

const openCreateModal = () => {
  patientToEdit.value = null
  showFormModal.value = true
}

const openEditModal = async (item: PatientListItem) => {
  try {
    const res = await patientApi.getPatientById(item.id)
    if (res.success && res.data) {
      patientToEdit.value = res.data
      showFormModal.value = true
    }
  } catch (err) {
    console.error('Error getting patient details:', err)
  }
}

const openMergeModal = (item: PatientListItem) => {
  targetPatientToMerge.value = item
  showMergeModal.value = true
}

const handleDelete = async (item: PatientListItem) => {
  if (confirm(`Bạn có chắc chắn muốn xóa hồ sơ bệnh nhân [${item.patientCode}] ${item.fullName}?`)) {
    try {
      const res = await patientApi.deletePatient(item.id)
      if (res.success) {
        fetchPatients()
      }
    } catch (err) {
      alert('Không thể xóa bệnh nhân này!')
    }
  }
}

const calculateAge = (dobString: string): number => {
  if (!dobString) return 0
  const dob = new Date(dobString)
  const diff = Date.now() - dob.getTime()
  const ageDate = new Date(diff)
  return Math.abs(ageDate.getUTCFullYear() - 1970)
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header banner -->
    <div class="flex flex-wrap items-center justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center text-white shadow-md">
          <Users class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Quản Lý Hồ Sơ Bệnh Nhân (MPI)</h1>
          <p class="text-xs text-slate-500 mt-0.5">
            Master Patient Index: Định danh CCCD, BHYT, phát hiện trùng lặp thời gian thực và hợp nhất hồ sơ y tế
          </p>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <button
          @click="openCreateModal"
          class="inline-flex items-center gap-2 px-4 py-2.5 bg-primary text-white text-xs font-bold rounded-xl hover:bg-primary-hover shadow-sm transition-all"
        >
          <UserPlus class="w-4 h-4" />
          <span>Tiếp đón / Thêm bệnh nhân</span>
        </button>
      </div>
    </div>

    <!-- Data Table Container -->
    <DataTable
      :columns="columns"
      :items="patients"
      :loading="loading"
      :total-count="totalCount"
      :page="page"
      :page-size="pageSize"
      search-placeholder="Tìm kiếm theo Mã BN, CCCD, BHYT, SĐT, Họ tên..."
      export-file-name="Danh_sach_benh_nhan"
      @update:search="handleSearch"
      @update:page="handlePageChange"
      @update:pageSize="handlePageSizeChange"
      @row-click="openEditModal"
    >
      <!-- Filters slot -->
      <template #filters>
        <select
          v-model="selectedGender"
          @change="fetchPatients"
          class="px-3 py-2 text-xs bg-white border border-slate-200 rounded-xl focus:ring-1 focus:ring-primary text-slate-700 shadow-xs"
        >
          <option :value="undefined">Tất cả giới tính</option>
          <option :value="Gender.Male">Nam</option>
          <option :value="Gender.Female">Nữ</option>
          <option :value="Gender.Other">Khác</option>
        </select>

        <label class="inline-flex items-center gap-1.5 text-xs text-slate-600 cursor-pointer select-none ml-2">
          <input
            v-model="includeMerged"
            type="checkbox"
            @change="fetchPatients"
            class="rounded border-slate-300 text-primary focus:ring-primary"
          />
          <span>Hiển thị đã gộp</span>
        </label>
      </template>

      <!-- Custom Cells -->
      <template #cell-patientCode="{ value }">
        <span class="font-mono font-bold text-primary">{{ value }}</span>
      </template>

      <template #cell-fullName="{ item }">
        <div>
          <span class="font-semibold text-slate-900 uppercase">{{ item.fullName }}</span>
          <div class="text-[11px] text-slate-400 font-normal">
            {{ item.email || 'Chưa cập nhật email' }}
          </div>
        </div>
      </template>

      <template #cell-dateOfBirth="{ value }">
        <span class="text-xs text-slate-700 font-medium">
          {{ value?.slice(0, 10) }}
          <span class="text-slate-400 font-normal">({{ calculateAge(value) }}t)</span>
        </span>
      </template>

      <template #cell-gender="{ value }">
        <span
          class="inline-flex px-2 py-0.5 rounded-full text-[11px] font-semibold"
          :class="value === Gender.Male ? 'bg-blue-100 text-blue-700' : value === Gender.Female ? 'bg-pink-100 text-pink-700' : 'bg-slate-100 text-slate-600'"
        >
          {{ value === Gender.Male ? 'Nam' : value === Gender.Female ? 'Nữ' : 'Khác' }}
        </span>
      </template>

      <template #cell-identityCardNumber="{ value }">
        <span v-if="value" class="font-mono text-xs text-slate-800 font-medium">{{ value }}</span>
        <span v-else class="text-xs text-slate-300 italic">Chưa có</span>
      </template>

      <template #cell-healthInsuranceCardNumber="{ value }">
        <span v-if="value" class="font-mono text-xs text-emerald-700 font-semibold bg-emerald-50 px-2 py-0.5 rounded">
          {{ value }}
        </span>
        <span v-else class="text-xs text-slate-300 italic">Chưa có</span>
      </template>

      <template #cell-phoneNumber="{ value }">
        <span v-if="value" class="text-xs text-slate-700 font-medium">{{ value }}</span>
        <span v-else class="text-xs text-slate-300 italic">—</span>
      </template>

      <template #cell-address="{ item }">
        <span class="text-xs text-slate-600 truncate max-w-[200px] block" :title="item.address">
          {{ item.address || [item.wardName, item.districtName, item.provinceName].filter(Boolean).join(', ') || '—' }}
        </span>
      </template>

      <template #cell-status="{ item }">
        <StatusBadge
          v-if="item.isMerged"
          status="Đã gộp"
          variant="warning"
        />
        <StatusBadge
          v-else
          status="Hoạt động"
          variant="success"
        />
      </template>

      <!-- Row Actions -->
      <template #actions="{ item }">
        <div class="flex items-center justify-end gap-1">
          <button
            @click.stop="openEditModal(item)"
            class="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
            title="Sửa hồ sơ"
          >
            <Edit class="w-4 h-4" />
          </button>
          <button
            v-if="!item.isMerged"
            @click.stop="openMergeModal(item)"
            class="p-1.5 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors"
            title="Gộp hồ sơ trùng"
          >
            <GitMerge class="w-4 h-4" />
          </button>
          <button
            @click.stop="handleDelete(item)"
            class="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
            title="Xóa hồ sơ"
          >
            <Trash2 class="w-4 h-4" />
          </button>
        </div>
      </template>
    </DataTable>

    <!-- Modals -->
    <PatientFormModal
      v-model="showFormModal"
      :patient-to-edit="patientToEdit"
      @saved="fetchPatients"
    />

    <PatientMergeModal
      v-model="showMergeModal"
      :target-patient="targetPatientToMerge"
      :all-patients="patients"
      @merged="fetchPatients"
    />
  </div>
</template>
