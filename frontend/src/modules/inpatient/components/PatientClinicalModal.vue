<script setup lang="ts">
import { ref, watch } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { inpatientApi } from '../api/inpatientApi'
import type { InpatientAdmission } from '../types/inpatient'
import { CareLevel, DischargeOutcome } from '../types/inpatient'
import { FileText, Activity, HeartPulse, DollarSign, LogOut } from 'lucide-vue-next'

const props = defineProps<{
  visible: boolean
  admissionId?: string
}>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'updated'): void
}>()

const activeTab = ref<'orders' | 'vitals' | 'care' | 'advances' | 'discharge'>('orders')
const loading = ref(false)
const error = ref('')
const successMsg = ref('')
const admission = ref<InpatientAdmission | null>(null)

// Form Y lệnh
const orderForm = ref({
  doctorName: 'BS.CKII Trần Đức Cường',
  diagnosis: '',
  careLevel: CareLevel.Level2,
  dietPlan: 'Cơm bệnh lý, giảm muối',
  medicinesContent: '1. Ceftriaxon 1g x 02 lọ (Tiêm TMC lúc 08h - 20h)\n2. Paracetamol 500mg x 02 viên (Uống khi sốt >= 38.5)',
  servicesContent: 'Khí dung Pulmicort 0.5mg x 2 cữ/ngày',
  specialInstructions: 'Theo dõi sinh hiệu mỗi 4h'
})

// Form Sinh hiệu
const vitalForm = ref({
  pulse: 80,
  temperature: 37.0,
  bloodPressureSystolic: 120,
  bloodPressureDiastolic: 80,
  breathingRate: 20,
  spO2: 98,
  weight: 65,
  notes: 'Ổn định, không khó thở',
  recordedByNurse: 'ĐD. Phạm Thị Mai'
})

// Form Phiếu chăm sóc
const careForm = ref({
  clinicalEvolution: 'Bệnh nhân tỉnh, tiếp xúc tốt, hết sốt, giảm ho',
  nursingActions: 'Thực hiện y lệnh thuốc tiêm đúng giờ, đo sinh hiệu, hướng dẫn ăn uống',
  evaluation: 'Bệnh nhân ổn định',
  nurseName: 'ĐD. Phạm Thị Mai'
})

// Form Tạm ứng
const advanceForm = ref({
  amount: 2000000,
  paymentMethod: 'TRANSFER',
  cashierName: 'Thu ngân nội trú',
  notes: 'Tạm ứng điều trị đợt 2'
})

// Form Xuất viện
const dischargeForm = ref({
  dischargeDiagnosis: '',
  dischargeIcd10Code: '',
  outcome: DischargeOutcome.Cured,
  dischargeSummaryNotes: 'Bệnh nhân khỏi bệnh, sinh hiệu ổn định, cho xuất viện nghỉ ngơi và hẹn tái khám sau 7 ngày',
  dischargeDoctorName: 'BS.CKII Trần Đức Cường'
})

async function fetchDetail() {
  if (!props.admissionId) return
  loading.value = true
  error.value = ''
  try {
    admission.value = await inpatientApi.getAdmissionDetail(props.admissionId)
    orderForm.value.diagnosis = admission.value.initialDiagnosis
    dischargeForm.value.dischargeDiagnosis = admission.value.initialDiagnosis
    dischargeForm.value.dischargeIcd10Code = admission.value.primaryIcd10Code
  } catch (err: any) {
    error.value = 'Lỗi tải chi tiết bệnh án'
  } finally {
    loading.value = false
  }
}

watch(() => props.visible, (val) => {
  if (val && props.admissionId) {
    fetchDetail()
  }
})

async function submitOrder() {
  if (!admission.value) return
  loading.value = true
  error.value = ''
  try {
    await inpatientApi.createMedicalOrder({
      admissionId: admission.value.id,
      doctorName: orderForm.value.doctorName,
      diagnosis: orderForm.value.diagnosis,
      careLevel: Number(orderForm.value.careLevel),
      dietPlan: orderForm.value.dietPlan,
      medicinesContent: orderForm.value.medicinesContent,
      servicesContent: orderForm.value.servicesContent,
      specialInstructions: orderForm.value.specialInstructions
    })
    successMsg.value = 'Đã lưu y lệnh điều trị thành công!'
    await fetchDetail()
    emit('updated')
  } catch (err: any) {
    error.value = 'Lỗi lưu y lệnh'
  } finally {
    loading.value = false
  }
}

async function submitVital() {
  if (!admission.value) return
  loading.value = true
  error.value = ''
  try {
    await inpatientApi.recordVitalSign({
      admissionId: admission.value.id,
      pulse: Number(vitalForm.value.pulse),
      temperature: Number(vitalForm.value.temperature),
      bloodPressureSystolic: Number(vitalForm.value.bloodPressureSystolic),
      bloodPressureDiastolic: Number(vitalForm.value.bloodPressureDiastolic),
      breathingRate: Number(vitalForm.value.breathingRate),
      spO2: Number(vitalForm.value.spO2),
      weight: Number(vitalForm.value.weight),
      notes: vitalForm.value.notes,
      recordedByNurse: vitalForm.value.recordedByNurse
    })
    successMsg.value = 'Đã lưu chỉ số sinh hiệu thành công!'
    await fetchDetail()
    emit('updated')
  } catch (err: any) {
    error.value = 'Lỗi lưu sinh hiệu'
  } finally {
    loading.value = false
  }
}

async function submitCareNote() {
  if (!admission.value) return
  loading.value = true
  error.value = ''
  try {
    await inpatientApi.createCareNote({
      admissionId: admission.value.id,
      clinicalEvolution: careForm.value.clinicalEvolution,
      nursingActions: careForm.value.nursingActions,
      evaluation: careForm.value.evaluation,
      nurseName: careForm.value.nurseName
    })
    successMsg.value = 'Đã lưu phiếu chăm sóc thành công!'
    await fetchDetail()
    emit('updated')
  } catch (err: any) {
    error.value = 'Lỗi lưu phiếu chăm sóc'
  } finally {
    loading.value = false
  }
}

async function submitAdvance() {
  if (!admission.value) return
  loading.value = true
  error.value = ''
  try {
    await inpatientApi.recordAdvancePayment({
      admissionId: admission.value.id,
      amount: Number(advanceForm.value.amount),
      paymentMethod: advanceForm.value.paymentMethod,
      cashierName: advanceForm.value.cashierName,
      notes: advanceForm.value.notes
    })
    successMsg.value = 'Đã thu tạm ứng viện phí thành công!'
    await fetchDetail()
    emit('updated')
  } catch (err: any) {
    error.value = 'Lỗi thu tạm ứng'
  } finally {
    loading.value = false
  }
}

async function submitDischarge() {
  if (!admission.value) return
  loading.value = true
  error.value = ''
  try {
    await inpatientApi.dischargeAdmission(admission.value.id, {
      admissionId: admission.value.id,
      dischargeDiagnosis: dischargeForm.value.dischargeDiagnosis,
      dischargeIcd10Code: dischargeForm.value.dischargeIcd10Code,
      outcome: Number(dischargeForm.value.outcome),
      dischargeSummaryNotes: dischargeForm.value.dischargeSummaryNotes,
      dischargeDoctorName: dischargeForm.value.dischargeDoctorName
    })
    successMsg.value = 'Bệnh nhân đã xuất viện và giải phóng giường bệnh thành công!'
    emit('updated')
    emit('update:visible', false)
  } catch (err: any) {
    error.value = 'Lỗi xử lý xuất viện'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="visible"
    title="Hồ sơ Bệnh án Nội trú & Trạm Điều trị"
    max-width="4xl"
    @update:model-value="emit('update:visible', $event)"
  >
    <div v-if="admission" class="space-y-4 text-xs">
      <!-- Inpatient Banner -->
      <div class="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-xl p-3 flex flex-wrap items-center justify-between gap-3">
        <div>
          <div class="font-bold text-slate-900 text-sm flex items-center gap-2">
            {{ admission.patientName }}
            <span class="text-xs px-2 py-0.5 rounded font-mono font-normal bg-blue-100 text-blue-800">
              {{ admission.admissionCode }}
            </span>
            <span class="text-xs px-2 py-0.5 rounded font-bold bg-amber-100 text-amber-800">
              Giường: {{ admission.currentBedCode }}
            </span>
          </div>
          <div class="text-slate-600 mt-0.5">
            {{ admission.gender }} | Khoa: {{ admission.departmentName }} | BHYT: {{ admission.healthInsuranceNumber || 'Không' }}
          </div>
          <div class="text-slate-500 mt-0.5">
            Chẩn đoán vào viện: <span class="font-semibold text-slate-800">{{ admission.initialDiagnosis }}</span> ({{ admission.primaryIcd10Code }})
          </div>
        </div>
        <div class="text-right">
          <div class="text-slate-500">Tổng tạm ứng: <span class="font-bold text-emerald-600 font-mono">{{ admission.totalAdvancePayment.toLocaleString('vi-VN') }} đ</span></div>
          <div class="text-slate-500">Chi phí dự tính: <span class="font-bold text-blue-600 font-mono">{{ admission.totalInpatientExpenses.toLocaleString('vi-VN') }} đ</span></div>
        </div>
      </div>

      <!-- Alerts -->
      <div v-if="error" class="p-2.5 bg-red-50 text-red-700 rounded-lg border border-red-200">
        {{ error }}
      </div>
      <div v-if="successMsg" class="p-2.5 bg-emerald-50 text-emerald-800 rounded-lg border border-emerald-200">
        {{ successMsg }}
      </div>

      <!-- Navigation Tabs -->
      <div class="flex border-b border-slate-200 gap-6 font-semibold">
        <button
          type="button"
          @click="activeTab = 'orders'"
          class="pb-2.5 flex items-center gap-1.5 border-b-2 transition-colors"
          :class="activeTab === 'orders' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <FileText class="w-4 h-4" />
          1. Y lệnh hàng ngày ({{ admission.medicalOrders.length }})
        </button>
        <button
          type="button"
          @click="activeTab = 'vitals'"
          class="pb-2.5 flex items-center gap-1.5 border-b-2 transition-colors"
          :class="activeTab === 'vitals' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <HeartPulse class="w-4 h-4" />
          2. Theo dõi sinh hiệu ({{ admission.vitalSigns.length }})
        </button>
        <button
          type="button"
          @click="activeTab = 'care'"
          class="pb-2.5 flex items-center gap-1.5 border-b-2 transition-colors"
          :class="activeTab === 'care' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <Activity class="w-4 h-4" />
          3. Phiếu chăm sóc ({{ admission.careNotes.length }})
        </button>
        <button
          type="button"
          @click="activeTab = 'advances'"
          class="pb-2.5 flex items-center gap-1.5 border-b-2 transition-colors"
          :class="activeTab === 'advances' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <DollarSign class="w-4 h-4" />
          4. Tạm ứng ({{ admission.advancePayments.length }})
        </button>
        <button
          type="button"
          @click="activeTab = 'discharge'"
          class="pb-2.5 flex items-center gap-1.5 border-b-2 transition-colors"
          :class="activeTab === 'discharge' ? 'border-red-600 text-red-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <LogOut class="w-4 h-4" />
          5. Xuất viện & Tổng kết
        </button>
      </div>

      <!-- Tab 1: Y lệnh -->
      <div v-if="activeTab === 'orders'" class="space-y-4">
        <!-- New Order Form -->
        <div class="bg-slate-50 p-3 rounded-xl border border-slate-200 space-y-3">
          <div class="font-bold text-slate-800">Ra y lệnh điều trị mới cho ngày hôm nay</div>
          <div class="grid grid-cols-3 gap-3">
            <div>
              <label class="font-semibold block mb-1">Bác sĩ ra y lệnh</label>
              <input v-model="orderForm.doctorName" type="text" class="w-full px-2.5 py-1.5 border rounded bg-white" />
            </div>
            <div>
              <label class="font-semibold block mb-1">Chế độ chăm sóc</label>
              <select v-model="orderForm.careLevel" class="w-full px-2.5 py-1.5 border rounded bg-white">
                <option :value="CareLevel.Level1">Cấp 1 (Nặng, bất động)</option>
                <option :value="CareLevel.Level2">Cấp 2 (Vận động hạn chế)</option>
                <option :value="CareLevel.Level3">Cấp 3 (Tự phục vụ)</option>
              </select>
            </div>
            <div>
              <label class="font-semibold block mb-1">Chế độ ăn bệnh lý</label>
              <input v-model="orderForm.dietPlan" type="text" class="w-full px-2.5 py-1.5 border rounded bg-white" />
            </div>
          </div>
          <div>
            <label class="font-semibold block mb-1">Thuốc & Hàm lượng điều trị *</label>
            <textarea v-model="orderForm.medicinesContent" rows="3" class="w-full px-2.5 py-1.5 border rounded bg-white font-mono"></textarea>
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="font-semibold block mb-1">Chỉ định CLS / Thăm dò chức năng</label>
              <input v-model="orderForm.servicesContent" type="text" class="w-full px-2.5 py-1.5 border rounded bg-white" />
            </div>
            <div>
              <label class="font-semibold block mb-1">Lời dặn & Hướng dẫn đặc biệt</label>
              <input v-model="orderForm.specialInstructions" type="text" class="w-full px-2.5 py-1.5 border rounded bg-white" />
            </div>
          </div>
          <div class="flex justify-end">
            <button type="button" @click="submitOrder" :disabled="loading" class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold shadow-sm">
              Lưu y lệnh
            </button>
          </div>
        </div>

        <!-- History of Orders -->
        <div class="space-y-2">
          <div class="font-bold text-slate-800">Lịch sử y lệnh điều trị</div>
          <div v-for="order in admission.medicalOrders" :key="order.id" class="p-3 bg-white border rounded-xl space-y-1.5">
            <div class="flex justify-between items-center text-slate-600">
              <span class="font-bold text-slate-900">{{ new Date(order.orderDate).toLocaleDateString('vi-VN') }} | {{ order.doctorName }}</span>
              <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-blue-50 text-blue-700 border border-blue-200">
                Chăm sóc Cấp {{ order.careLevel }}
              </span>
            </div>
            <div class="font-mono bg-slate-50 p-2 rounded text-slate-800 whitespace-pre-line">{{ order.medicinesContent }}</div>
            <div v-if="order.dietPlan" class="text-slate-600">Chế độ ăn: {{ order.dietPlan }}</div>
          </div>
        </div>
      </div>

      <!-- Tab 2: Sinh hiệu -->
      <div v-if="activeTab === 'vitals'" class="space-y-4">
        <!-- New Vital Form -->
        <div class="bg-slate-50 p-3 rounded-xl border border-slate-200 space-y-2">
          <div class="font-bold text-slate-800">Ghi nhận chỉ số sinh hiệu (Điều dưỡng)</div>
          <div class="grid grid-cols-3 md:grid-cols-6 gap-2">
            <div>
              <label class="text-slate-500 block mb-0.5">Mạch (ck/p)</label>
              <input v-model="vitalForm.pulse" type="number" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">Nhiệt độ (°C)</label>
              <input v-model="vitalForm.temperature" type="number" step="0.1" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">HA tối đa (mmHg)</label>
              <input v-model="vitalForm.bloodPressureSystolic" type="number" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">HA tối thiểu (mmHg)</label>
              <input v-model="vitalForm.bloodPressureDiastolic" type="number" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">Nhịp thở (lần/p)</label>
              <input v-model="vitalForm.breathingRate" type="number" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">SpO2 (%)</label>
              <input v-model="vitalForm.spO2" type="number" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
          </div>
          <div class="flex justify-end pt-1">
            <button type="button" @click="submitVital" :disabled="loading" class="px-4 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold">
              Lưu sinh hiệu
            </button>
          </div>
        </div>

        <!-- Vitals Table -->
        <div class="border rounded-xl overflow-hidden">
          <table class="w-full text-left border-collapse">
            <thead class="bg-slate-100 font-bold text-slate-700">
              <tr>
                <th class="p-2">Thời gian</th>
                <th class="p-2 text-center">Mạch (ck/p)</th>
                <th class="p-2 text-center">Nhiệt độ (°C)</th>
                <th class="p-2 text-center">Huyết áp (mmHg)</th>
                <th class="p-2 text-center">Nhịp thở</th>
                <th class="p-2 text-center">SpO2 (%)</th>
                <th class="p-2">Điều dưỡng</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <tr v-for="v in admission.vitalSigns" :key="v.id" class="hover:bg-slate-50">
                <td class="p-2 text-slate-600 font-mono">{{ new Date(v.recordTime).toLocaleString('vi-VN') }}</td>
                <td class="p-2 text-center font-bold text-rose-600">{{ v.pulse }}</td>
                <td class="p-2 text-center font-bold text-amber-600">{{ v.temperature }}</td>
                <td class="p-2 text-center font-bold text-blue-600">{{ v.bloodPressureSystolic }}/{{ v.bloodPressureDiastolic }}</td>
                <td class="p-2 text-center">{{ v.breathingRate }}</td>
                <td class="p-2 text-center font-bold text-emerald-600">{{ v.spO2 }}%</td>
                <td class="p-2 text-slate-600">{{ v.recordedByNurse }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Tab 3: Phiếu chăm sóc -->
      <div v-if="activeTab === 'care'" class="space-y-4">
        <div class="bg-slate-50 p-3 rounded-xl border border-slate-200 space-y-2">
          <div class="font-bold text-slate-800">Ghi nhận phiếu chăm sóc điều dưỡng</div>
          <div class="grid grid-cols-3 gap-2">
            <div>
              <label class="font-semibold block mb-0.5">Diễn biến người bệnh *</label>
              <textarea v-model="careForm.clinicalEvolution" rows="2" class="w-full px-2 py-1 border rounded bg-white"></textarea>
            </div>
            <div>
              <label class="font-semibold block mb-0.5">Xử trí / Can thiệp điều dưỡng *</label>
              <textarea v-model="careForm.nursingActions" rows="2" class="w-full px-2 py-1 border rounded bg-white"></textarea>
            </div>
            <div>
              <label class="font-semibold block mb-0.5">Đánh giá kết quả *</label>
              <textarea v-model="careForm.evaluation" rows="2" class="w-full px-2 py-1 border rounded bg-white"></textarea>
            </div>
          </div>
          <div class="flex justify-end pt-1">
            <button type="button" @click="submitCareNote" :disabled="loading" class="px-4 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-semibold">
              Lưu phiếu chăm sóc
            </button>
          </div>
        </div>

        <div class="space-y-2">
          <div v-for="c in admission.careNotes" :key="c.id" class="p-3 bg-white border rounded-xl">
            <div class="flex justify-between font-bold text-slate-800 mb-1">
              <span>{{ new Date(c.noteTime).toLocaleString('vi-VN') }}</span>
              <span class="text-slate-500 font-normal">Điều dưỡng: {{ c.nurseName }}</span>
            </div>
            <div class="text-slate-700"><span class="font-semibold">Diễn biến:</span> {{ c.clinicalEvolution }}</div>
            <div class="text-slate-700"><span class="font-semibold">Thực hiện y lệnh:</span> {{ c.nursingActions }}</div>
            <div class="text-emerald-700 font-medium"><span class="font-semibold">Đánh giá:</span> {{ c.evaluation }}</div>
          </div>
        </div>
      </div>

      <!-- Tab 4: Tạm ứng -->
      <div v-if="activeTab === 'advances'" class="space-y-4">
        <div class="bg-slate-50 p-3 rounded-xl border border-slate-200 flex gap-3 items-end">
          <div class="flex-1">
            <label class="font-semibold block mb-1">Số tiền tạm ứng thêm (VNĐ)</label>
            <input v-model.number="advanceForm.amount" type="number" step="500000" class="w-full px-3 py-2 border rounded-lg bg-white font-mono font-bold text-emerald-700" />
          </div>
          <div class="w-48">
            <label class="font-semibold block mb-1">Hình thức</label>
            <select v-model="advanceForm.paymentMethod" class="w-full px-3 py-2 border rounded-lg bg-white">
              <option value="CASH">Tiền mặt</option>
              <option value="TRANSFER">Chuyển khoản</option>
            </select>
          </div>
          <button type="button" @click="submitAdvance" :disabled="loading" class="px-5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg font-semibold shadow-sm">
            Thu tạm ứng
          </button>
        </div>

        <div class="border rounded-xl overflow-hidden">
          <table class="w-full text-left border-collapse">
            <thead class="bg-slate-100 font-bold text-slate-700">
              <tr>
                <th class="p-2">Số biên lai</th>
                <th class="p-2">Ngày nộp</th>
                <th class="p-2 text-right">Số tiền</th>
                <th class="p-2">Hình thức</th>
                <th class="p-2">Thu ngân</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <tr v-for="p in admission.advancePayments" :key="p.id">
                <td class="p-2 font-mono font-bold text-slate-800">{{ p.receiptNumber }}</td>
                <td class="p-2 text-slate-600">{{ new Date(p.paymentDate).toLocaleString('vi-VN') }}</td>
                <td class="p-2 text-right font-mono font-bold text-emerald-600">{{ p.amount.toLocaleString('vi-VN') }} đ</td>
                <td class="p-2 text-slate-600">{{ p.paymentMethod }}</td>
                <td class="p-2 text-slate-600">{{ p.cashierName }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Tab 5: Xuất viện -->
      <div v-if="activeTab === 'discharge'" class="p-4 bg-red-50/40 border border-red-200 rounded-xl space-y-3">
        <div class="font-bold text-red-900 text-sm">Tổng kết Bệnh án & Quyết định Xuất viện</div>
        <div class="grid grid-cols-12 gap-2">
          <div class="col-span-3">
            <label class="font-semibold block mb-1">Mã ICD-10 ra viện *</label>
            <input v-model="dischargeForm.dischargeIcd10Code" type="text" class="w-full px-2 py-1.5 border rounded bg-white font-mono font-bold text-red-700" />
          </div>
          <div class="col-span-9">
            <label class="font-semibold block mb-1">Chẩn đoán ra viện *</label>
            <input v-model="dischargeForm.dischargeDiagnosis" type="text" class="w-full px-3 py-1.5 border rounded bg-white" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="font-semibold block mb-1">Kết quả điều trị *</label>
            <select v-model="dischargeForm.outcome" class="w-full px-3 py-1.5 border rounded bg-white">
              <option :value="DischargeOutcome.Cured">Khỏi bệnh</option>
              <option :value="DischargeOutcome.Improved">Đỡ / Giảm</option>
              <option :value="DischargeOutcome.Unchanged">Không thay đổi</option>
              <option :value="DischargeOutcome.Worsened">Nặng hơn</option>
              <option :value="DischargeOutcome.Deceased">Tử vong</option>
            </select>
          </div>
          <div>
            <label class="font-semibold block mb-1">Bác sĩ ký tổng kết</label>
            <input v-model="dischargeForm.dischargeDoctorName" type="text" class="w-full px-3 py-1.5 border rounded bg-white" />
          </div>
        </div>

        <div>
          <label class="font-semibold block mb-1">Tóm tắt quá trình điều trị & Lời dặn khi ra viện</label>
          <textarea v-model="dischargeForm.dischargeSummaryNotes" rows="3" class="w-full px-3 py-2 border rounded bg-white"></textarea>
        </div>

        <div class="p-3 bg-amber-50 rounded-lg border border-amber-200 text-amber-800 text-[11px]">
          * Lưu ý: Khi xác nhận xuất viện, hệ thống sẽ tự động giải phóng giường <span class="font-bold">{{ admission.currentBedCode }}</span> và chuyển sang trạng thái "Đang dọn vệ sinh / Khử khuẩn".
        </div>

        <div class="flex justify-end pt-2">
          <button
            type="button"
            @click="submitDischarge"
            :disabled="loading"
            class="px-5 py-2.5 bg-red-600 hover:bg-red-700 text-white rounded-lg font-bold shadow-md transition-colors"
          >
            Xác nhận Xuất viện & Giải phóng Giường
          </button>
        </div>
      </div>
    </div>
  </BaseModal>
</template>
