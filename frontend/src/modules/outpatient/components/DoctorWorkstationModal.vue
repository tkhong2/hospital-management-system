<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { outpatientApi } from '../api/outpatientApi'
import type { Reception, Encounter } from '../types/outpatient'
import { TreatmentDisposition } from '../types/outpatient'
import { Stethoscope, FlaskConical, Pill, Printer, CheckCircle } from 'lucide-vue-next'

const props = defineProps<{
  visible: boolean
  reception: Reception | null
}>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'completed'): void
}>()

const activeTab = ref<'exam' | 'services' | 'prescription'>('exam')
const loading = ref(false)
const encounter = ref<Encounter | null>(null)
const error = ref('')
const successMsg = ref('')

// Form chẩn đoán & sinh hiệu
const examForm = ref({
  chiefComplaint: '',
  historyOfPresentIllness: '',
  medicalHistory: '',
  physicalExam: '',
  pulse: 78,
  bloodPressureSystolic: 120,
  bloodPressureDiastolic: 80,
  temperature: 36.8,
  spO2: 98,
  weight: 65,
  height: 165,
  primaryIcd10Code: 'I10',
  primaryIcd10Name: 'Bệnh tăng huyết áp vô căn (nguyên phát)',
  secondaryIcd10Code: '',
  secondaryIcd10Name: '',
  clinicalDiagnosis: '',
  disposition: TreatmentDisposition.DischargedWithPrescription,
  doctorNotes: 'Ăn nhạt, nghỉ ngơi, kiêng chất kích thích',
  followUpDate: ''
})

const bmi = computed(() => {
  if (!examForm.value.height || !examForm.value.weight) return 0
  const h = examForm.value.height / 100
  return Math.round((examForm.value.weight / (h * h)) * 10) / 10
})

const commonIcd10 = [
  { code: 'I10', name: 'Bệnh tăng huyết áp vô căn (nguyên phát)' },
  { code: 'J18.9', name: 'Viêm phổi không đặc hiệu' },
  { code: 'E11.9', name: 'Đái tháo đường type 2 không biến chứng' },
  { code: 'K29.7', name: 'Viêm dạ dày không đặc hiệu' },
  { code: 'J02.9', name: 'Viêm họng cấp không đặc hiệu' },
  { code: 'M54.5', name: 'Đau thắt lưng (đau cột sống thắt lưng)' }
]

function onIcdSelect(code: string) {
  const found = commonIcd10.find(i => i.code === code)
  if (found) {
    examForm.value.primaryIcd10Code = found.code
    examForm.value.primaryIcd10Name = found.name
  }
}

// Chỉ định dịch vụ
const availableServices = [
  { id: '11111111-1111-1111-1111-111111111111', code: 'XN-CTM', name: 'Tổng phân tích tế bào máu ngoại vi bằng máy đếm laser', group: 'XET_NGHIEM', price: 90000, room: 'P. Xét nghiệm 1' },
  { id: '22222222-2222-2222-2222-222222222222', code: 'XN-SHM', name: 'Sinh hóa máu (Glucose, Ure, Creatinin, GOT, GPT)', group: 'XET_NGHIEM', price: 180000, room: 'P. Xét nghiệm 2' },
  { id: '33333333-3333-3333-3333-333333333333', code: 'CDHA-XQ', name: 'Chụp X-Quang ngực thẳng kỹ thuật số (CR/DR)', group: 'CDHA', price: 120000, room: 'P. X-Quang 01' },
  { id: '44444444-4444-4444-4444-444444444444', code: 'CDHA-SA', name: 'Siêu âm Doppler màu tim / ổ bụng tổng quát', group: 'CDHA', price: 250000, room: 'P. Siêu âm 03' },
  { id: '55555555-5555-5555-5555-555555555555', code: 'TDCN-ECG', name: 'Ghi điện tim 12 chuyển đạo', group: 'THAM_DO_CN', price: 70000, room: 'P. Điện tim' }
]

const selectedServiceIds = ref<string[]>([])

// Kê đơn thuốc
const availableMedicines = [
  { id: 'aaaa1111-1111-1111-1111-111111111111', code: 'MED-AMLO', name: 'Amlodipin 5mg', active: 'Amlodipine', form: 'Viên', price: 4000, defaultDose: '1 viên', instruct: 'Uống 1 viên vào 08h sáng sau ăn' },
  { id: 'bbbb2222-2222-2222-2222-222222222222', code: 'MED-LOSAR', name: 'Losartan 50mg', active: 'Losartan potassium', form: 'Viên', price: 6500, defaultDose: '1 viên', instruct: 'Uống 1 viên sáng sau ăn' },
  { id: 'cccc3333-3333-3333-3333-333333333333', code: 'MED-PANAD', name: 'Panadol Extra 500mg', active: 'Paracetamol + Caffeine', form: 'Viên', price: 2500, defaultDose: '1 viên', instruct: 'Uống khi đau đầu hoặc sốt >= 38.5' },
  { id: 'dddd4444-4444-4444-4444-444444444444', code: 'MED-NEXIUM', name: 'Nexium Mups 40mg', active: 'Esomeprazole', form: 'Viên', price: 22000, defaultDose: '1 viên', instruct: 'Uống trước ăn sáng 30 phút' }
]

const prescriptionItems = ref<Array<{
  medicineId: string
  medicineCode: string
  medicineName: string
  activeIngredient: string
  dosageForm: string
  quantity: number
  morningDose: string
  noonDose: string
  afternoonDose: string
  nightDose: string
  usageInstructions: string
  unitPrice: number
  isInsuranceCovered: boolean
}>>([])

watch(() => props.visible, async (val) => {
  if (val && props.reception) {
    loading.value = true
    error.value = ''
    successMsg.value = ''
    try {
      const enc = await outpatientApi.startEncounter({
        receptionId: props.reception.id,
        doctorId: 'dr_01',
        doctorName: 'BS.CKII Nguyễn Thanh Tùng'
      })
      encounter.value = enc

      examForm.value.chiefComplaint = enc.chiefComplaint || props.reception.reason || ''
      if (enc.pulse) examForm.value.pulse = enc.pulse
      if (enc.bloodPressureSystolic) examForm.value.bloodPressureSystolic = enc.bloodPressureSystolic
      if (enc.bloodPressureDiastolic) examForm.value.bloodPressureDiastolic = enc.bloodPressureDiastolic
      if (enc.temperature) examForm.value.temperature = enc.temperature
      if (enc.spO2) examForm.value.spO2 = enc.spO2
      if (enc.primaryIcd10Code) {
        examForm.value.primaryIcd10Code = enc.primaryIcd10Code
        examForm.value.primaryIcd10Name = enc.primaryIcd10Name
      }
    } catch (e: any) {
      error.value = 'Lỗi khởi tạo phiên khám'
    } finally {
      loading.value = false
    }
  }
})

function addPrescriptionMedicine(med: typeof availableMedicines[0]) {
  if (prescriptionItems.value.some(p => p.medicineId === med.id)) return
  prescriptionItems.value.push({
    medicineId: med.id,
    medicineCode: med.code,
    medicineName: med.name,
    activeIngredient: med.active,
    dosageForm: med.form,
    quantity: 14,
    morningDose: med.defaultDose,
    noonDose: '',
    afternoonDose: '',
    nightDose: '',
    usageInstructions: med.instruct,
    unitPrice: med.price,
    isInsuranceCovered: true
  })
}

function removePrescriptionItem(idx: number) {
  prescriptionItems.value.splice(idx, 1)
}

async function saveDiagnosis() {
  if (!encounter.value) return
  loading.value = true
  error.value = ''
  try {
    const updated = await outpatientApi.updateDiagnosis(encounter.value.id, {
      encounterId: encounter.value.id,
      chiefComplaint: examForm.value.chiefComplaint,
      historyOfPresentIllness: examForm.value.historyOfPresentIllness,
      medicalHistory: examForm.value.medicalHistory,
      physicalExam: examForm.value.physicalExam,
      pulse: examForm.value.pulse,
      bloodPressureSystolic: examForm.value.bloodPressureSystolic,
      bloodPressureDiastolic: examForm.value.bloodPressureDiastolic,
      temperature: examForm.value.temperature,
      spO2: examForm.value.spO2,
      weight: examForm.value.weight,
      height: examForm.value.height,
      primaryIcd10Code: examForm.value.primaryIcd10Code,
      primaryIcd10Name: examForm.value.primaryIcd10Name,
      secondaryIcd10Code: examForm.value.secondaryIcd10Code || undefined,
      secondaryIcd10Name: examForm.value.secondaryIcd10Name || undefined,
      clinicalDiagnosis: examForm.value.clinicalDiagnosis,
      disposition: Number(examForm.value.disposition),
      doctorNotes: examForm.value.doctorNotes,
      followUpDate: examForm.value.followUpDate || undefined
    })
    encounter.value = updated
    successMsg.value = 'Đã lưu kết quả khám lâm sàng & ICD-10 thành công!'
  } catch (err: any) {
    error.value = 'Lỗi lưu thông tin chẩn đoán'
  } finally {
    loading.value = false
  }
}

async function saveServiceOrders() {
  if (!encounter.value || selectedServiceIds.value.length === 0) return
  loading.value = true
  try {
    const items = selectedServiceIds.value.map(id => {
      const s = availableServices.find(x => x.id === id)!
      return {
        serviceId: s.id,
        serviceCode: s.code,
        serviceName: s.name,
        serviceGroup: s.group,
        quantity: 1,
        unitPrice: s.price,
        insuranceCoverageRate: 0.8,
        roomToExecute: s.room
      }
    })
    const order = await outpatientApi.createServiceOrder({
      encounterId: encounter.value.id,
      doctorName: encounter.value.doctorName,
      items
    })
    if (encounter.value) {
      encounter.value.serviceOrders.push(order)
    }
    successMsg.value = `Đã chỉ định thành công ${items.length} dịch vụ kỹ thuật!`
  } catch (err: any) {
    error.value = 'Lỗi chỉ định dịch vụ kỹ thuật'
  } finally {
    loading.value = false
  }
}

async function savePrescription() {
  if (!encounter.value || prescriptionItems.value.length === 0) return
  loading.value = true
  try {
    const pres = await outpatientApi.createPrescription({
      encounterId: encounter.value.id,
      doctorName: encounter.value.doctorName,
      diagnosis: examForm.value.primaryIcd10Name,
      daysCount: 7,
      advice: examForm.value.doctorNotes,
      items: prescriptionItems.value
    })
    if (encounter.value) {
      encounter.value.prescriptions.push(pres)
    }
    successMsg.value = 'Đã kê đơn thuốc điện tử thành công!'
  } catch (err: any) {
    error.value = 'Lỗi lưu đơn thuốc'
  } finally {
    loading.value = false
  }
}

async function handleCompleteEncounter() {
  if (!encounter.value) return
  loading.value = true
  try {
    await saveDiagnosis()
    if (prescriptionItems.value.length > 0) {
      await savePrescription()
    }
    await outpatientApi.completeEncounter(encounter.value.id)
    emit('completed')
    emit('update:visible', false)
  } catch (err: any) {
    error.value = 'Lỗi hoàn tất lượt khám'
  } finally {
    loading.value = false
  }
}

function printPrescription() {
  window.print()
}
</script>

<template>
  <BaseModal
    :model-value="visible"
    title="Buồng khám Bác sĩ - Doctor Workstation"
    max-width="4xl"
    @update:model-value="emit('update:visible', $event)"
  >
    <div v-if="reception" class="space-y-4">
      <!-- Patient Banner -->
      <div class="bg-gradient-to-r from-blue-50 to-indigo-50 border border-blue-200 rounded-xl p-3 flex flex-wrap items-center justify-between gap-3 text-xs">
        <div class="flex items-center gap-3">
          <div class="w-9 h-9 rounded-lg bg-blue-600 text-white font-bold flex items-center justify-center text-sm">
            {{ reception.queueNumber }}
          </div>
          <div>
            <div class="font-bold text-slate-900 text-sm flex items-center gap-2">
              {{ reception.patientName }}
              <span class="text-xs px-2 py-0.5 rounded font-mono font-normal bg-blue-100 text-blue-800">
                {{ reception.patientCode }}
              </span>
            </div>
            <div class="text-slate-600 mt-0.5">
              {{ reception.gender }} | BHYT: {{ reception.healthInsuranceNumber || 'Không' }} | Phòng: {{ reception.clinicRoomName }}
            </div>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <button
            type="button"
            @click="printPrescription"
            class="px-3 py-1.5 border border-slate-300 bg-white hover:bg-slate-50 text-slate-700 rounded-lg flex items-center gap-1.5 font-medium shadow-sm"
          >
            <Printer class="w-3.5 h-3.5" />
            In phiếu
          </button>
          <button
            type="button"
            @click="handleCompleteEncounter"
            :disabled="loading"
            class="px-4 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg flex items-center gap-1.5 font-medium shadow-sm transition-colors"
          >
            <CheckCircle class="w-3.5 h-3.5" />
            Hoàn tất khám
          </button>
        </div>
      </div>

      <!-- Alerts -->
      <div v-if="error" class="p-2.5 bg-red-50 text-red-700 text-xs rounded-lg border border-red-200">
        {{ error }}
      </div>
      <div v-if="successMsg" class="p-2.5 bg-emerald-50 text-emerald-800 text-xs rounded-lg border border-emerald-200">
        {{ successMsg }}
      </div>

      <!-- Tabs Navigation -->
      <div class="flex border-b border-slate-200 gap-6 text-xs font-semibold">
        <button
          type="button"
          @click="activeTab = 'exam'"
          class="pb-2.5 flex items-center gap-1.5 border-b-2 transition-colors"
          :class="activeTab === 'exam' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <Stethoscope class="w-4 h-4" />
          1. Khám bệnh & ICD-10
        </button>
        <button
          type="button"
          @click="activeTab = 'services'"
          class="pb-2.5 flex items-center gap-1.5 border-b-2 transition-colors"
          :class="activeTab === 'services' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <FlaskConical class="w-4 h-4" />
          2. Chỉ định CLS ({{ selectedServiceIds.length }})
        </button>
        <button
          type="button"
          @click="activeTab = 'prescription'"
          class="pb-2.5 flex items-center gap-1.5 border-b-2 transition-colors"
          :class="activeTab === 'prescription' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-500 hover:text-slate-700'"
        >
          <Pill class="w-4 h-4" />
          3. Kê đơn thuốc ({{ prescriptionItems.length }})
        </button>
      </div>

      <!-- Tab 1: Khám lâm sàng & ICD-10 -->
      <div v-if="activeTab === 'exam'" class="space-y-4">
        <!-- Vitals Grid -->
        <div class="bg-slate-50 p-3 rounded-xl border border-slate-200">
          <div class="text-xs font-bold text-slate-700 mb-2">Bảng sinh hiệu & Chỉ số cơ thể</div>
          <div class="grid grid-cols-2 md:grid-cols-6 gap-2 text-xs">
            <div>
              <label class="text-slate-500 block mb-0.5">Mạch (ck/p)</label>
              <input v-model="examForm.pulse" type="number" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">HA tâm thu (mmHg)</label>
              <input v-model="examForm.bloodPressureSystolic" type="number" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">HA tâm trương (mmHg)</label>
              <input v-model="examForm.bloodPressureDiastolic" type="number" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">Thân nhiệt (°C)</label>
              <input v-model="examForm.temperature" type="number" step="0.1" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">SpO2 (%)</label>
              <input v-model="examForm.spO2" type="number" class="w-full px-2 py-1 border rounded bg-white" />
            </div>
            <div>
              <label class="text-slate-500 block mb-0.5">BMI (kg/m²)</label>
              <div class="px-2 py-1 bg-slate-200 font-bold text-slate-800 rounded">{{ bmi }}</div>
            </div>
          </div>
        </div>

        <!-- Clinical Exam Inputs -->
        <div class="grid grid-cols-2 gap-3 text-xs">
          <div>
            <label class="font-semibold text-slate-700 block mb-1">Lý do vào khám *</label>
            <input v-model="examForm.chiefComplaint" type="text" class="w-full px-3 py-2 border rounded-lg" />
          </div>
          <div>
            <label class="font-semibold text-slate-700 block mb-1">Quá trình bệnh lý (Bệnh sử)</label>
            <input v-model="examForm.historyOfPresentIllness" type="text" placeholder="Khởi phát cách 3 ngày, sốt, ho khan..." class="w-full px-3 py-2 border rounded-lg" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-3 text-xs">
          <div>
            <label class="font-semibold text-slate-700 block mb-1">Khám các cơ quan (Tim mạch, Hô hấp, Tiêu hóa)</label>
            <textarea v-model="examForm.physicalExam" rows="2" placeholder="Tim đều, phổi không rale, bụng mềm..." class="w-full px-3 py-2 border rounded-lg"></textarea>
          </div>
          <div>
            <label class="font-semibold text-slate-700 block mb-1">Tiền sử bản thân / Dị ứng</label>
            <textarea v-model="examForm.medicalHistory" rows="2" placeholder="Tăng HA, không dị ứng thuốc..." class="w-full px-3 py-2 border rounded-lg"></textarea>
          </div>
        </div>

        <!-- ICD-10 Section -->
        <div class="p-3 bg-blue-50/60 border border-blue-200 rounded-xl space-y-2 text-xs">
          <div class="flex items-center justify-between">
            <span class="font-bold text-blue-900">Chẩn đoán theo mã bệnh ICD-10</span>
            <div class="flex gap-1.5 flex-wrap">
              <button
                v-for="item in commonIcd10"
                :key="item.code"
                type="button"
                @click="onIcdSelect(item.code)"
                class="px-2 py-0.5 bg-white border border-blue-200 rounded text-[11px] text-blue-700 hover:bg-blue-100"
              >
                {{ item.code }}
              </button>
            </div>
          </div>
          <div class="grid grid-cols-12 gap-2">
            <div class="col-span-3">
              <label class="text-slate-600 block mb-0.5 font-medium">Mã ICD-10 chính *</label>
              <input v-model="examForm.primaryIcd10Code" type="text" class="w-full px-2 py-1.5 border rounded bg-white font-mono font-bold text-blue-700" />
            </div>
            <div class="col-span-9">
              <label class="text-slate-600 block mb-0.5 font-medium">Tên bệnh chẩn đoán chính *</label>
              <input v-model="examForm.primaryIcd10Name" type="text" class="w-full px-2 py-1.5 border rounded bg-white" />
            </div>
          </div>
        </div>

        <!-- Treatment Disposition -->
        <div class="grid grid-cols-3 gap-3 text-xs">
          <div>
            <label class="font-semibold text-slate-700 block mb-1">Hướng xử trí</label>
            <select v-model="examForm.disposition" class="w-full px-3 py-2 border rounded-lg">
              <option :value="TreatmentDisposition.DischargedWithPrescription">Cho về điều trị ngoại trú</option>
              <option :value="TreatmentDisposition.Hospitalize">Chỉ định nhập viện nội trú</option>
              <option :value="TreatmentDisposition.TransferHospital">Chuyển viện tuyến trên</option>
              <option :value="TreatmentDisposition.FollowUp">Hẹn khám lại</option>
            </select>
          </div>
          <div>
            <label class="font-semibold text-slate-700 block mb-1">Lời dặn của bác sĩ</label>
            <input v-model="examForm.doctorNotes" type="text" class="w-full px-3 py-2 border rounded-lg" />
          </div>
          <div>
            <label class="font-semibold text-slate-700 block mb-1">Ngày hẹn tái khám</label>
            <input v-model="examForm.followUpDate" type="date" class="w-full px-3 py-2 border rounded-lg" />
          </div>
        </div>

        <div class="flex justify-end pt-2">
          <button
            type="button"
            @click="saveDiagnosis"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-semibold shadow-sm"
          >
            Lưu chẩn đoán & Sinh hiệu
          </button>
        </div>
      </div>

      <!-- Tab 2: Chỉ định dịch vụ CLS -->
      <div v-if="activeTab === 'services'" class="space-y-4 text-xs">
        <div class="border rounded-xl p-3 bg-slate-50">
          <div class="font-bold text-slate-800 mb-2">Chọn dịch vụ kỹ thuật / Cận lâm sàng</div>
          <div class="space-y-2">
            <label
              v-for="s in availableServices"
              :key="s.id"
              class="flex items-center justify-between p-2.5 bg-white border rounded-lg hover:border-blue-400 cursor-pointer"
            >
              <div class="flex items-center gap-2">
                <input type="checkbox" :value="s.id" v-model="selectedServiceIds" class="rounded text-blue-600" />
                <div>
                  <span class="font-bold text-slate-800">{{ s.name }}</span>
                  <span class="text-[11px] text-slate-500 block font-mono">{{ s.code }} | {{ s.room }}</span>
                </div>
              </div>
              <div class="text-right">
                <span class="font-bold text-blue-600">{{ s.price.toLocaleString('vi-VN') }} đ</span>
                <span class="text-[10px] text-emerald-600 block">BHYT trả 80%</span>
              </div>
            </label>
          </div>
        </div>

        <div class="flex justify-end">
          <button
            type="button"
            @click="saveServiceOrders"
            :disabled="selectedServiceIds.length === 0"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white rounded-lg font-semibold shadow-sm"
          >
            Ký gửi chỉ định CLS ({{ selectedServiceIds.length }})
          </button>
        </div>
      </div>

      <!-- Tab 3: Kê đơn thuốc -->
      <div v-if="activeTab === 'prescription'" class="space-y-4 text-xs">
        <div class="bg-slate-50 p-3 rounded-xl border">
          <div class="font-bold text-slate-800 mb-2">Danh mục thuốc khả dụng trong kho ngoại trú (FEFO)</div>
          <div class="flex flex-wrap gap-2">
            <button
              v-for="m in availableMedicines"
              :key="m.id"
              type="button"
              @click="addPrescriptionMedicine(m)"
              class="px-2.5 py-1.5 bg-white border border-slate-300 hover:border-blue-500 rounded-lg text-slate-800 font-medium flex items-center gap-1.5 shadow-sm"
            >
              + {{ m.name }}
              <span class="text-[10px] text-blue-600">({{ m.price.toLocaleString('vi-VN') }}đ)</span>
            </button>
          </div>
        </div>

        <!-- Selected Prescription Items Table -->
        <div v-if="prescriptionItems.length > 0" class="border rounded-xl overflow-hidden">
          <table class="w-full text-left border-collapse">
            <thead class="bg-slate-100 text-[11px] font-bold text-slate-700">
              <tr>
                <th class="p-2.5">Tên thuốc & Hoạt chất</th>
                <th class="p-2.5 w-20 text-center">Số lượng</th>
                <th class="p-2.5 w-28 text-center">Liều dùng</th>
                <th class="p-2.5">Cách dùng</th>
                <th class="p-2.5 w-24 text-right">Đơn giá</th>
                <th class="p-2.5 w-12 text-center">Xóa</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-200">
              <tr v-for="(item, idx) in prescriptionItems" :key="item.medicineId" class="hover:bg-slate-50">
                <td class="p-2.5">
                  <div class="font-bold text-slate-900">{{ item.medicineName }}</div>
                  <div class="text-[11px] text-slate-500">{{ item.activeIngredient }} ({{ item.dosageForm }})</div>
                </td>
                <td class="p-2.5 text-center">
                  <input v-model.number="item.quantity" type="number" min="1" class="w-16 px-1.5 py-1 border rounded text-center" />
                </td>
                <td class="p-2.5 text-center">
                  <input v-model="item.morningDose" type="text" placeholder="Sáng" class="w-20 px-1 py-1 border rounded text-center text-xs" />
                </td>
                <td class="p-2.5">
                  <input v-model="item.usageInstructions" type="text" class="w-full px-2 py-1 border rounded text-xs" />
                </td>
                <td class="p-2.5 text-right font-mono font-medium text-slate-700">
                  {{ (item.unitPrice * item.quantity).toLocaleString('vi-VN') }} đ
                </td>
                <td class="p-2.5 text-center">
                  <button type="button" @click="removePrescriptionItem(idx)" class="text-red-500 hover:text-red-700 font-bold">×</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="p-6 text-center text-slate-400 bg-slate-50 rounded-xl border border-dashed">
          Chưa chọn thuốc nào cho đơn này. Vui lòng bấm vào các thuốc ở trên để thêm vào toa.
        </div>

        <div class="flex justify-end">
          <button
            type="button"
            @click="savePrescription"
            :disabled="prescriptionItems.length === 0"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white rounded-lg font-semibold shadow-sm"
          >
            Lưu & Ký đơn thuốc điện tử
          </button>
        </div>
      </div>
    </div>
  </BaseModal>
</template>
