<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import {
  Bed,
  UserPlus,
  RefreshCw,
  Activity,
  Layers,
  ArrowRightLeft,
  Sparkles,
  Wrench,
  AlertCircle
} from 'lucide-vue-next'
import { inpatientApi } from '../api/inpatientApi'
import type { BedMapSummary, HospitalBed, InpatientAdmission } from '../types/inpatient'
import { BedStatus } from '../types/inpatient'
import AdmissionModal from '../components/AdmissionModal.vue'
import PatientClinicalModal from '../components/PatientClinicalModal.vue'

const activeTab = ref<'bedmap' | 'admissions'>('bedmap')
const loading = ref(false)
const bedMap = ref<BedMapSummary | null>(null)
const selectedDept = ref('')
const activeAdmissions = ref<InpatientAdmission[]>([])

const showAdmissionModal = ref(false)
const showClinicalModal = ref(false)
const selectedAdmissionId = ref<string | undefined>(undefined)
const preselectedBedId = ref<string | undefined>(undefined)

const toastMsg = ref('')
const toastType = ref<'success' | 'error'>('success')

// Drag & Drop State
const draggedBed = ref<HospitalBed | null>(null)

async function fetchBedMap() {
  loading.value = true
  try {
    bedMap.value = await inpatientApi.getBedMap(selectedDept.value || undefined)
    activeAdmissions.value = await inpatientApi.getActiveAdmissions(selectedDept.value || undefined)
  } catch (err) {
    console.error('Lỗi tải sơ đồ giường:', err)
  } finally {
    loading.value = false
  }
}

const allBeds = computed(() => {
  if (!bedMap.value) return []
  return bedMap.value.rooms.flatMap(r => r.beds)
})

function openAdmissionForBed(bed: HospitalBed) {
  preselectedBedId.value = bed.id
  showAdmissionModal.value = true
}

function openClinicalStation(admissionId: string) {
  selectedAdmissionId.value = admissionId
  showClinicalModal.value = true
}

// Drag and drop handlers
function onDragStart(event: DragEvent, bed: HospitalBed) {
  if (bed.status !== BedStatus.Occupied || !bed.currentAdmissionId) {
    event.preventDefault()
    return
  }
  draggedBed.value = bed
  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move'
    event.dataTransfer.setData('text/plain', bed.currentAdmissionId)
  }
}

async function onDropBed(event: DragEvent, targetBed: HospitalBed) {
  event.preventDefault()
  if (!draggedBed.value || !draggedBed.value.currentAdmissionId) return

  if (targetBed.status !== BedStatus.Empty) {
    showToast(`Giường đích ${targetBed.bedCode} không trống!`, 'error')
    draggedBed.value = null
    return
  }

  const admId = draggedBed.value.currentAdmissionId
  const fromBedCode = draggedBed.value.bedCode
  const toBedCode = targetBed.bedCode
  draggedBed.value = null

  loading.value = true
  try {
    await inpatientApi.transferBed({
      admissionId: admId,
      targetBedId: targetBed.id,
      reason: `Kéo thả chuyển giường từ ${fromBedCode} sang ${toBedCode}`,
      transferredBy: 'Điều dưỡng trưởng ca'
    })
    showToast(`Đã chuyển bệnh nhân từ giường ${fromBedCode} sang ${toBedCode} thành công!`, 'success')
    await fetchBedMap()
  } catch (err: any) {
    showToast(err.response?.data?.errors?.[0] || 'Lỗi khi chuyển giường', 'error')
  } finally {
    loading.value = false
  }
}

async function markBedCleaningDone(bedId: string) {
  loading.value = true
  try {
    await inpatientApi.updateBedStatus(bedId, BedStatus.Empty)
    showToast('Đã khử trùng xong! Giường đã sẵn sàng đón bệnh nhân mới.', 'success')
    await fetchBedMap()
  } catch (err: any) {
    showToast('Lỗi cập nhật trạng thái giường', 'error')
  } finally {
    loading.value = false
  }
}

async function toggleBedMaintenance(bed: HospitalBed) {
  loading.value = true
  const newStatus = bed.status === BedStatus.Maintenance ? BedStatus.Empty : BedStatus.Maintenance
  try {
    await inpatientApi.updateBedStatus(bed.id, newStatus, newStatus === BedStatus.Maintenance ? 'Báo hỏng sửa chữa' : 'Đã sửa xong')
    showToast(newStatus === BedStatus.Maintenance ? 'Đã đưa giường vào diện bảo trì' : 'Đã hoàn tất bảo trì giường', 'success')
    await fetchBedMap()
  } catch (err: any) {
    showToast('Lỗi cập nhật bảo trì', 'error')
  } finally {
    loading.value = false
  }
}

function showToast(msg: string, type: 'success' | 'error' = 'success') {
  toastMsg.value = msg
  toastType.value = type
  setTimeout(() => {
    toastMsg.value = ''
  }, 4000)
}

function getBedCardColor(status: BedStatus) {
  switch (status) {
    case BedStatus.Empty:
      return 'bg-emerald-50/70 border-emerald-300 hover:border-emerald-500'
    case BedStatus.Occupied:
      return 'bg-rose-50/70 border-rose-300 hover:border-rose-500 shadow-sm cursor-grab active:cursor-grabbing'
    case BedStatus.Cleaning:
      return 'bg-amber-50/70 border-amber-300 hover:border-amber-500'
    case BedStatus.Maintenance:
      return 'bg-slate-100 border-slate-300 text-slate-500'
    default:
      return 'bg-white border-slate-200'
  }
}

onMounted(() => {
  fetchBedMap()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header Banner -->
    <div class="flex items-center justify-between bg-white p-5 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-indigo-600 to-purple-700 flex items-center justify-center text-white shadow-md">
          <Bed class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Phân hệ Quản Lý Nội Trú (Inpatient)</h1>
          <p class="text-xs text-slate-500 mt-0.5">Sơ đồ giường trực quan Drag & Drop, chuyển khoa/giường, y lệnh, sinh hiệu & tổng kết bệnh án</p>
        </div>
      </div>
      <div class="flex items-center gap-2">
        <select
          v-model="selectedDept"
          @change="fetchBedMap"
          class="text-xs px-3 py-2 border border-slate-200 rounded-xl bg-slate-50 font-medium"
        >
          <option value="">Tất cả các khoa</option>
          <option value="KHOA_NOI">Khoa Nội Tổng Hợp</option>
          <option value="KHOA_NGOAI">Khoa Ngoại Tổng Hợp</option>
        </select>
        <button
          type="button"
          @click="fetchBedMap"
          class="p-2 border border-slate-200 rounded-xl text-slate-600 hover:bg-slate-50"
          title="Tải lại sơ đồ giường"
        >
          <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />
        </button>
        <button
          type="button"
          @click="showAdmissionModal = true"
          class="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold shadow-sm flex items-center gap-2 transition-colors"
        >
          <UserPlus class="w-4 h-4" />
          Tiếp nhận nhập viện mới
        </button>
      </div>
    </div>

    <!-- Toast Notification -->
    <div
      v-if="toastMsg"
      class="p-3 text-xs rounded-xl border flex items-center gap-2 transition-all shadow-sm"
      :class="toastType === 'success' ? 'bg-emerald-50 text-emerald-800 border-emerald-200' : 'bg-red-50 text-red-800 border-red-200'"
    >
      <AlertCircle class="w-4 h-4 flex-shrink-0" />
      {{ toastMsg }}
    </div>

    <!-- Statistics Cards -->
    <div v-if="bedMap" class="grid grid-cols-2 md:grid-cols-6 gap-3">
      <div class="bg-white p-3.5 rounded-xl border border-slate-200 shadow-sm">
        <div class="text-slate-500 text-[11px]">Tổng số giường</div>
        <div class="text-xl font-bold text-slate-900 mt-0.5">{{ bedMap.totalBeds }}</div>
      </div>
      <div class="bg-emerald-50/60 p-3.5 rounded-xl border border-emerald-200 shadow-sm">
        <div class="text-emerald-700 text-[11px] font-medium">Giường trống</div>
        <div class="text-xl font-bold text-emerald-700 mt-0.5">{{ bedMap.emptyBeds }}</div>
      </div>
      <div class="bg-rose-50/60 p-3.5 rounded-xl border border-rose-200 shadow-sm">
        <div class="text-rose-700 text-[11px] font-medium">Đang điều trị</div>
        <div class="text-xl font-bold text-rose-700 mt-0.5">{{ bedMap.occupiedBeds }}</div>
      </div>
      <div class="bg-amber-50/60 p-3.5 rounded-xl border border-amber-200 shadow-sm">
        <div class="text-amber-700 text-[11px] font-medium">Đang dọn vệ sinh</div>
        <div class="text-xl font-bold text-amber-700 mt-0.5">{{ bedMap.cleaningBeds }}</div>
      </div>
      <div class="bg-slate-100 p-3.5 rounded-xl border border-slate-300 shadow-sm">
        <div class="text-slate-600 text-[11px] font-medium">Đang bảo trì</div>
        <div class="text-xl font-bold text-slate-700 mt-0.5">{{ bedMap.maintenanceBeds }}</div>
      </div>
      <div class="bg-indigo-50/60 p-3.5 rounded-xl border border-indigo-200 shadow-sm">
        <div class="text-indigo-700 text-[11px] font-medium">Công suất sử dụng</div>
        <div class="text-xl font-bold text-indigo-700 mt-0.5">{{ bedMap.occupancyRate }}%</div>
      </div>
    </div>

    <!-- Main Content Tabs -->
    <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
      <div class="flex border-b border-slate-200 px-6 pt-3 gap-6 text-xs font-semibold">
        <button
          type="button"
          @click="activeTab = 'bedmap'"
          class="pb-3 border-b-2 flex items-center gap-2 transition-colors"
          :class="activeTab === 'bedmap' ? 'border-indigo-600 text-indigo-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <Layers class="w-4 h-4" />
          Sơ đồ Buồng / Giường trực quan (Drag & Drop)
        </button>
        <button
          type="button"
          @click="activeTab = 'admissions'"
          class="pb-3 border-b-2 flex items-center gap-2 transition-colors"
          :class="activeTab === 'admissions' ? 'border-indigo-600 text-indigo-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <Activity class="w-4 h-4" />
          Danh sách Bệnh nhân Đang Nằm Viện ({{ activeAdmissions.length }})
        </button>
      </div>

      <!-- Tab 1: Visual Bed Map -->
      <div v-if="activeTab === 'bedmap'" class="p-6 space-y-6">
        <div class="p-3 bg-indigo-50/60 border border-indigo-200 rounded-xl text-xs text-indigo-900 flex items-center gap-2">
          <ArrowRightLeft class="w-4 h-4 flex-shrink-0 text-indigo-600" />
          <span>
            <strong>Gợi ý thao tác:</strong> Bạn có thể <strong>kéo thả trực tiếp</strong> một thẻ giường màu đỏ (có bệnh nhân) thả vào bất kỳ thẻ giường màu xanh (trống) nào để chuyển giường bệnh nhân tức thì!
          </span>
        </div>

        <div v-for="room in bedMap?.rooms" :key="room.id" class="border border-slate-200 rounded-2xl p-4 bg-slate-50/50 space-y-3">
          <div class="flex justify-between items-center">
            <div class="flex items-center gap-2">
              <span class="px-2 py-0.5 rounded font-mono font-bold bg-slate-200 text-slate-800 text-xs">
                {{ room.roomNumber }}
              </span>
              <span class="font-bold text-slate-900 text-sm">{{ room.roomName }}</span>
              <span class="text-xs text-slate-500">({{ room.departmentName }} - Tầng {{ room.floor }})</span>
            </div>
            <div class="text-xs text-slate-500 font-medium">
              {{ room.beds.filter(b => b.status === BedStatus.Occupied).length }}/{{ room.beds.length }} giường đang nằm
            </div>
          </div>

          <!-- Beds Grid in Room -->
          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-3">
            <div
              v-for="bed in room.beds"
              :key="bed.id"
              class="border-2 rounded-xl p-3 flex flex-col justify-between transition-all select-none"
              :class="getBedCardColor(bed.status)"
              :draggable="bed.status === BedStatus.Occupied"
              @dragstart="onDragStart($event, bed)"
              @dragover.prevent
              @dragenter.prevent
              @drop="onDropBed($event, bed)"
            >
              <div>
                <div class="flex items-center justify-between">
                  <span class="font-mono font-bold text-xs">{{ bed.bedCode }}</span>
                  <span
                    class="px-1.5 py-0.5 rounded text-[10px] font-bold uppercase"
                    :class="{
                      'bg-emerald-200 text-emerald-800': bed.status === BedStatus.Empty,
                      'bg-rose-200 text-rose-800': bed.status === BedStatus.Occupied,
                      'bg-amber-200 text-amber-800': bed.status === BedStatus.Cleaning,
                      'bg-slate-300 text-slate-700': bed.status === BedStatus.Maintenance
                    }"
                  >
                    {{ bed.status === BedStatus.Empty ? 'Trống' : bed.status === BedStatus.Occupied ? 'Có người' : bed.status === BedStatus.Cleaning ? 'Đang dọn' : 'Bảo trì' }}
                  </span>
                </div>

                <!-- Occupied Details -->
                <div v-if="bed.status === BedStatus.Occupied" class="mt-2 text-xs space-y-1">
                  <div class="font-bold text-slate-900 truncate">{{ bed.patientName }}</div>
                  <div class="text-[11px] text-slate-600 font-mono">{{ bed.patientCode }}</div>
                  <div class="text-[11px] text-slate-600 truncate" :title="bed.initialDiagnosis">
                    {{ bed.initialDiagnosis }}
                  </div>
                </div>

                <!-- Empty Details -->
                <div v-else-if="bed.status === BedStatus.Empty" class="mt-2 text-xs text-emerald-800">
                  <div class="font-medium">{{ bed.bedName }}</div>
                  <div class="text-[11px] text-slate-500 font-mono">{{ bed.dailyPrice.toLocaleString('vi-VN') }} đ/ngày</div>
                </div>

                <!-- Cleaning Details -->
                <div v-else-if="bed.status === BedStatus.Cleaning" class="mt-2 text-xs text-amber-800">
                  <div class="font-medium flex items-center gap-1">
                    <Sparkles class="w-3.5 h-3.5" />
                    Đang khử trùng
                  </div>
                  <div class="text-[11px] text-slate-500">Khử khuẩn buồng bệnh</div>
                </div>

                <!-- Maintenance Details -->
                <div v-else class="mt-2 text-xs text-slate-600">
                  <div class="font-medium flex items-center gap-1">
                    <Wrench class="w-3.5 h-3.5" />
                    Bảo trì kỹ thuật
                  </div>
                </div>
              </div>

              <!-- Action buttons per bed status -->
              <div class="mt-3 pt-2 border-t border-slate-200/60 flex items-center justify-between text-[11px]">
                <template v-if="bed.status === BedStatus.Occupied && bed.currentAdmissionId">
                  <button
                    type="button"
                    @click="openClinicalStation(bed.currentAdmissionId)"
                    class="px-2 py-1 bg-white border border-rose-300 hover:bg-rose-50 text-rose-700 font-bold rounded shadow-xs"
                  >
                    Bệnh án & Y lệnh
                  </button>
                  <span class="text-[10px] text-slate-400 italic">Kéo để chuyển</span>
                </template>

                <template v-else-if="bed.status === BedStatus.Empty">
                  <button
                    type="button"
                    @click="openAdmissionForBed(bed)"
                    class="px-2 py-1 bg-emerald-600 hover:bg-emerald-700 text-white font-medium rounded shadow-xs"
                  >
                    + Xếp giường
                  </button>
                  <button
                    type="button"
                    @click="toggleBedMaintenance(bed)"
                    class="text-slate-400 hover:text-slate-600"
                    title="Báo bảo trì"
                  >
                    <Wrench class="w-3.5 h-3.5" />
                  </button>
                </template>

                <template v-else-if="bed.status === BedStatus.Cleaning">
                  <button
                    type="button"
                    @click="markBedCleaningDone(bed.id)"
                    class="px-2 py-1 bg-amber-600 hover:bg-amber-700 text-white font-medium rounded shadow-xs"
                  >
                    Dọn xong
                  </button>
                </template>

                <template v-else-if="bed.status === BedStatus.Maintenance">
                  <button
                    type="button"
                    @click="toggleBedMaintenance(bed)"
                    class="px-2 py-1 bg-slate-600 hover:bg-slate-700 text-white font-medium rounded shadow-xs"
                  >
                    Đã sửa xong
                  </button>
                </template>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tab 2: Admissions List -->
      <div v-if="activeTab === 'admissions'" class="p-5">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse text-xs">
            <thead class="bg-slate-50 text-slate-600 font-bold border-b border-slate-200">
              <tr>
                <th class="p-3">Số Bệnh Án</th>
                <th class="p-3">Bệnh nhân</th>
                <th class="p-3">Khoa điều trị</th>
                <th class="p-3 text-center">Giường</th>
                <th class="p-3">Chẩn đoán vào viện</th>
                <th class="p-3 text-right">Tạm ứng</th>
                <th class="p-3 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="adm in activeAdmissions" :key="adm.id" class="hover:bg-slate-50">
                <td class="p-3 font-mono font-bold text-indigo-700">{{ adm.admissionCode }}</td>
                <td class="p-3">
                  <div class="font-bold text-slate-900">{{ adm.patientName }}</div>
                  <div class="text-[11px] text-slate-500 font-mono">{{ adm.patientCode }} | {{ adm.gender }}</div>
                </td>
                <td class="p-3 font-medium text-slate-700">{{ adm.departmentName }}</td>
                <td class="p-3 text-center font-bold font-mono text-blue-700">{{ adm.currentBedCode }}</td>
                <td class="p-3 text-slate-700 max-w-xs truncate">{{ adm.initialDiagnosis }}</td>
                <td class="p-3 text-right font-mono font-bold text-emerald-600">
                  {{ adm.totalAdvancePayment.toLocaleString('vi-VN') }} đ
                </td>
                <td class="p-3 text-right">
                  <button
                    type="button"
                    @click="openClinicalStation(adm.id)"
                    class="px-3 py-1.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg font-medium shadow-sm transition-colors text-xs"
                  >
                    Xem & Điều trị
                  </button>
                </td>
              </tr>
              <tr v-if="activeAdmissions.length === 0">
                <td colspan="7" class="p-8 text-center text-slate-400">
                  Không có bệnh nhân nào đang nằm viện trong danh sách lọc.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <AdmissionModal
      :visible="showAdmissionModal"
      :available-beds="allBeds"
      :preselected-bed-id="preselectedBedId"
      @update:visible="showAdmissionModal = $event"
      @admitted="fetchBedMap"
    />

    <PatientClinicalModal
      :visible="showClinicalModal"
      :admission-id="selectedAdmissionId"
      @update:visible="showClinicalModal = $event"
      @updated="fetchBedMap"
    />
  </div>
</template>
