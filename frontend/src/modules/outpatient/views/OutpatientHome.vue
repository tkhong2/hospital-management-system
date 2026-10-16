<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { Stethoscope, UserPlus, Users, Receipt, CheckCircle, Clock, Search, RefreshCw } from 'lucide-vue-next'
import { outpatientApi } from '../api/outpatientApi'
import type { Reception, Billing } from '../types/outpatient'
import { ReceptionStatus, PatientType } from '../types/outpatient'
import RegisterReceptionModal from '../components/RegisterReceptionModal.vue'
import DoctorWorkstationModal from '../components/DoctorWorkstationModal.vue'

const activeTab = ref<'queue' | 'billing' | 'history'>('queue')
const loading = ref(false)
const queueList = ref<Reception[]>([])
const selectedReception = ref<Reception | null>(null)

const showRegisterModal = ref(false)
const showDoctorModal = ref(false)

// Billing state
const billingLoading = ref(false)
const billingResult = ref<Billing | null>(null)
const billingMsg = ref('')

async function fetchQueue() {
  loading.value = true
  try {
    queueList.value = await outpatientApi.getQueue()
  } catch (e) {
    console.error('Failed to fetch queue:', e)
  } finally {
    loading.value = false
  }
}

function openDoctorWorkstation(reception: Reception) {
  selectedReception.value = reception
  showDoctorModal.value = true
}

async function processPayment(encounterId: string) {
  billingLoading.value = true
  billingMsg.value = ''
  try {
    const bill = await outpatientApi.processBilling({
      encounterId,
      paymentMethod: 'CASH',
      cashierName: 'Thu ngân quầy ngoại trú'
    })
    billingResult.value = bill
    billingMsg.value = `Đã thu viện phí thành công! Mã hóa đơn: ${bill.invoiceNumber}`
    await fetchQueue()
  } catch (err: any) {
    billingMsg.value = 'Lỗi xử lý thanh toán viện phí'
  } finally {
    billingLoading.value = false
  }
}

function getStatusBadgeClass(status: ReceptionStatus) {
  switch (status) {
    case ReceptionStatus.Waiting:
      return 'bg-amber-100 text-amber-800 border-amber-200'
    case ReceptionStatus.InConsultation:
      return 'bg-blue-100 text-blue-800 border-blue-200 animate-pulse'
    case ReceptionStatus.WaitingForLab:
      return 'bg-purple-100 text-purple-800 border-purple-200'
    case ReceptionStatus.Completed:
      return 'bg-emerald-100 text-emerald-800 border-emerald-200'
    default:
      return 'bg-slate-100 text-slate-800 border-slate-200'
  }
}

function getStatusText(status: ReceptionStatus) {
  switch (status) {
    case ReceptionStatus.Waiting:
      return 'Chờ khám'
    case ReceptionStatus.InConsultation:
      return 'Đang khám'
    case ReceptionStatus.WaitingForLab:
      return 'Chờ kết quả CLS'
    case ReceptionStatus.Completed:
      return 'Hoàn tất'
    case ReceptionStatus.Cancelled:
      return 'Đã hủy'
    default:
      return 'Không xác định'
  }
}

onMounted(() => {
  fetchQueue()
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header Banner -->
    <div class="flex items-center justify-between bg-white p-5 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-blue-600 to-indigo-700 flex items-center justify-center text-white shadow-md">
          <Stethoscope class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Phân hệ Khám Ngoại Trú (Outpatient)</h1>
          <p class="text-xs text-slate-500 mt-0.5">Tiếp đón, hàng chờ phòng khám, chẩn đoán ICD-10, chỉ định cận lâm sàng & viện phí</p>
        </div>
      </div>
      <div class="flex items-center gap-2">
        <button
          type="button"
          @click="fetchQueue"
          class="p-2 border border-slate-200 rounded-xl text-slate-600 hover:bg-slate-50"
          title="Làm mới hàng chờ"
        >
          <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />
        </button>
        <button
          type="button"
          @click="showRegisterModal = true"
          class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-xs font-semibold shadow-sm flex items-center gap-2 transition-colors"
        >
          <UserPlus class="w-4 h-4" />
          Tiếp đón bệnh nhân mới
        </button>
      </div>
    </div>

    <!-- Summary Statistics Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <div class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center gap-3">
        <div class="w-10 h-10 rounded-lg bg-blue-50 text-blue-600 flex items-center justify-center">
          <Users class="w-5 h-5" />
        </div>
        <div>
          <div class="text-xl font-bold text-slate-900">{{ queueList.length }}</div>
          <div class="text-xs text-slate-500">Tổng tiếp đón hôm nay</div>
        </div>
      </div>

      <div class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center gap-3">
        <div class="w-10 h-10 rounded-lg bg-amber-50 text-amber-600 flex items-center justify-center">
          <Clock class="w-5 h-5" />
        </div>
        <div>
          <div class="text-xl font-bold text-amber-600">
            {{ queueList.filter(q => q.status === ReceptionStatus.Waiting).length }}
          </div>
          <div class="text-xs text-slate-500">Đang chờ gọi khám</div>
        </div>
      </div>

      <div class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center gap-3">
        <div class="w-10 h-10 rounded-lg bg-purple-50 text-purple-600 flex items-center justify-center">
          <Stethoscope class="w-5 h-5" />
        </div>
        <div>
          <div class="text-xl font-bold text-purple-600">
            {{ queueList.filter(q => q.status === ReceptionStatus.InConsultation || q.status === ReceptionStatus.WaitingForLab).length }}
          </div>
          <div class="text-xs text-slate-500">Đang khám & Chờ CLS</div>
        </div>
      </div>

      <div class="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex items-center gap-3">
        <div class="w-10 h-10 rounded-lg bg-emerald-50 text-emerald-600 flex items-center justify-center">
          <CheckCircle class="w-5 h-5" />
        </div>
        <div>
          <div class="text-xl font-bold text-emerald-600">
            {{ queueList.filter(q => q.status === ReceptionStatus.Completed).length }}
          </div>
          <div class="text-xs text-slate-500">Đã hoàn tất khám</div>
        </div>
      </div>
    </div>

    <!-- Main Navigation Tabs -->
    <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
      <div class="flex border-b border-slate-200 px-6 pt-3 gap-6 text-xs font-semibold">
        <button
          type="button"
          @click="activeTab = 'queue'"
          class="pb-3 border-b-2 flex items-center gap-2 transition-colors"
          :class="activeTab === 'queue' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <Clock class="w-4 h-4" />
          Hàng chờ Phòng Khám (Queue Board)
        </button>
        <button
          type="button"
          @click="activeTab = 'billing'"
          class="pb-3 border-b-2 flex items-center gap-2 transition-colors"
          :class="activeTab === 'billing' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <Receipt class="w-4 h-4" />
          Quầy Viện Phí & BHYT
        </button>
      </div>

      <!-- Tab 1: Queue Board -->
      <div v-if="activeTab === 'queue'" class="p-5">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse text-xs">
            <thead class="bg-slate-50 text-slate-600 font-bold border-b border-slate-200">
              <tr>
                <th class="p-3 w-16 text-center">STT</th>
                <th class="p-3">Mã & Họ tên người bệnh</th>
                <th class="p-3">Phòng khám</th>
                <th class="p-3">Đối tượng</th>
                <th class="p-3">Lý do khám</th>
                <th class="p-3">Thời gian</th>
                <th class="p-3 text-center">Trạng thái</th>
                <th class="p-3 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="item in queueList" :key="item.id" class="hover:bg-slate-50/80 transition-colors">
                <td class="p-3 text-center font-bold text-blue-600 text-sm">
                  {{ item.queueNumber }}
                  <span v-if="item.priority > 0" class="block text-[10px] text-red-600 font-bold">Ưu tiên</span>
                </td>
                <td class="p-3">
                  <div class="font-bold text-slate-900">{{ item.patientName }}</div>
                  <div class="text-[11px] text-slate-500 font-mono">
                    {{ item.patientCode }} | {{ item.gender }}
                  </div>
                </td>
                <td class="p-3 font-medium text-slate-700">
                  {{ item.clinicRoomName }}
                </td>
                <td class="p-3">
                  <span
                    class="px-2 py-0.5 rounded text-[11px] font-medium"
                    :class="item.patientType === PatientType.OutpatientInsurance ? 'bg-blue-50 text-blue-700 border border-blue-200' : 'bg-slate-100 text-slate-700'"
                  >
                    {{ item.patientType === PatientType.OutpatientInsurance ? 'BHYT' : 'Viện phí' }}
                  </span>
                </td>
                <td class="p-3 text-slate-600 max-w-xs truncate">
                  {{ item.reason || 'Khám sức khỏe' }}
                </td>
                <td class="p-3 text-slate-500">
                  {{ new Date(item.receptionTime).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) }}
                </td>
                <td class="p-3 text-center">
                  <span
                    class="px-2.5 py-1 rounded-full text-[11px] font-medium border"
                    :class="getStatusBadgeClass(item.status)"
                  >
                    {{ getStatusText(item.status) }}
                  </span>
                </td>
                <td class="p-3 text-right">
                  <button
                    type="button"
                    @click="openDoctorWorkstation(item)"
                    class="px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium shadow-sm transition-colors text-xs"
                  >
                    Khám bệnh
                  </button>
                </td>
              </tr>
              <tr v-if="queueList.length === 0">
                <td colspan="8" class="p-8 text-center text-slate-400">
                  Chưa có bệnh nhân nào trong hàng chờ tiếp đón hôm nay.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Tab 2: Billing & Insurance -->
      <div v-if="activeTab === 'billing'" class="p-5 space-y-4">
        <div v-if="billingMsg" class="p-3 bg-emerald-50 text-emerald-800 text-xs rounded-xl border border-emerald-200">
          {{ billingMsg }}
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse text-xs">
            <thead class="bg-slate-50 text-slate-600 font-bold border-b border-slate-200">
              <tr>
                <th class="p-3">Mã bệnh nhân</th>
                <th class="p-3">Họ và tên</th>
                <th class="p-3">Phòng khám</th>
                <th class="p-3">Đối tượng</th>
                <th class="p-3 text-center">Trạng thái khám</th>
                <th class="p-3 text-right">Viện phí & BHYT</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-for="item in queueList" :key="item.id" class="hover:bg-slate-50">
                <td class="p-3 font-mono font-medium text-slate-700">{{ item.patientCode }}</td>
                <td class="p-3 font-bold text-slate-900">{{ item.patientName }}</td>
                <td class="p-3">{{ item.clinicRoomName }}</td>
                <td class="p-3">
                  {{ item.patientType === PatientType.OutpatientInsurance ? 'BHYT chi trả 80%' : '100% Viện phí' }}
                </td>
                <td class="p-3 text-center">
                  <span class="px-2 py-0.5 rounded text-[11px]" :class="getStatusBadgeClass(item.status)">
                    {{ getStatusText(item.status) }}
                  </span>
                </td>
                <td class="p-3 text-right">
                  <button
                    v-if="item.encounterId"
                    type="button"
                    @click="processPayment(item.encounterId)"
                    :disabled="billingLoading"
                    class="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg font-medium shadow-sm transition-colors text-xs"
                  >
                    Thu viện phí & In hóa đơn
                  </button>
                  <span v-else class="text-slate-400 italic">Chưa phát sinh chi phí</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <RegisterReceptionModal
      :visible="showRegisterModal"
      @update:visible="showRegisterModal = $event"
      @registered="fetchQueue"
    />

    <DoctorWorkstationModal
      :visible="showDoctorModal"
      :reception="selectedReception"
      @update:visible="showDoctorModal = $event"
      @completed="fetchQueue"
    />
  </div>
</template>
