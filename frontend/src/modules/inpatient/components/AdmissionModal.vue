<script setup lang="ts">
import { ref, computed } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { inpatientApi } from '../api/inpatientApi'
import type { HospitalBed } from '../types/inpatient'

const props = defineProps<{
  visible: boolean
  availableBeds: HospitalBed[]
  preselectedBedId?: string
}>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'admitted'): void
}>()

const loading = ref(false)
const error = ref('')

const form = ref({
  patientCode: 'BN26' + Math.floor(100000 + Math.random() * 900000),
  patientName: '',
  dateOfBirth: '1985-06-15',
  gender: 'Nam',
  healthInsuranceNumber: '',
  relativeContact: '',
  departmentCode: 'KHOA_NOI',
  departmentName: 'Khoa Nội Tổng Hợp',
  targetBedId: props.preselectedBedId || '',
  admittingDoctorName: 'BS.CKII Trần Đức Cường',
  initialDiagnosis: 'Viêm phổi thùy cấp',
  primaryIcd10Code: 'J18.9',
  primaryIcd10Name: 'Viêm phổi không đặc hiệu',
  initialAdvancePayment: 3000000
})

const filteredBeds = computed(() => {
  return props.availableBeds.filter(b => b.status === 0) // Empty beds
})

async function handleAdmission() {
  if (!form.value.patientName.trim()) {
    error.value = 'Vui lòng nhập tên bệnh nhân'
    return
  }
  if (!form.value.targetBedId) {
    error.value = 'Vui lòng chọn giường bệnh tiếp nhận'
    return
  }

  loading.value = true
  error.value = ''
  try {
    const bed = props.availableBeds.find(b => b.id === form.value.targetBedId)
    await inpatientApi.createAdmission({
      patientId: 'b2c3d4e5-f6a7-8901-2345-678901bcdefa',
      patientCode: form.value.patientCode,
      patientName: form.value.patientName,
      dateOfBirth: form.value.dateOfBirth,
      gender: form.value.gender,
      healthInsuranceNumber: form.value.healthInsuranceNumber,
      relativeContact: form.value.relativeContact,
      departmentCode: form.value.departmentCode,
      departmentName: bed ? bed.departmentName : form.value.departmentName,
      targetBedId: form.value.targetBedId,
      admittingDoctorName: form.value.admittingDoctorName,
      initialDiagnosis: form.value.initialDiagnosis,
      primaryIcd10Code: form.value.primaryIcd10Code,
      primaryIcd10Name: form.value.primaryIcd10Name,
      initialAdvancePayment: Number(form.value.initialAdvancePayment)
    })

    emit('admitted')
    emit('update:visible', false)
  } catch (err: any) {
    error.value = err.response?.data?.errors?.[0] || 'Lỗi khi tiếp nhận nhập viện'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="visible"
    title="Tiếp nhận Bệnh nhân Nhập viện Nội trú"
    @update:model-value="emit('update:visible', $event)"
  >
    <div class="space-y-4 text-xs">
      <div v-if="error" class="p-3 bg-red-50 text-red-700 rounded-lg border border-red-200">
        {{ error }}
      </div>

      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="block font-semibold text-slate-700 mb-1">Mã bệnh nhân *</label>
          <input v-model="form.patientCode" type="text" class="w-full px-3 py-2 border rounded-lg bg-slate-50 font-mono" />
        </div>
        <div>
          <label class="block font-semibold text-slate-700 mb-1">Họ và tên *</label>
          <input v-model="form.patientName" type="text" placeholder="Nguyễn Văn A" class="w-full px-3 py-2 border rounded-lg" />
        </div>
      </div>

      <div class="grid grid-cols-3 gap-3">
        <div>
          <label class="block font-semibold text-slate-700 mb-1">Ngày sinh</label>
          <input v-model="form.dateOfBirth" type="date" class="w-full px-3 py-2 border rounded-lg" />
        </div>
        <div>
          <label class="block font-semibold text-slate-700 mb-1">Giới tính</label>
          <select v-model="form.gender" class="w-full px-3 py-2 border rounded-lg">
            <option value="Nam">Nam</option>
            <option value="Nữ">Nữ</option>
            <option value="Khác">Khác</option>
          </select>
        </div>
        <div>
          <label class="block font-semibold text-slate-700 mb-1">Mã thẻ BHYT</label>
          <input v-model="form.healthInsuranceNumber" type="text" placeholder="HT201..." class="w-full px-3 py-2 border rounded-lg font-mono uppercase" />
        </div>
      </div>

      <div>
        <label class="block font-semibold text-slate-700 mb-1">Người nhà liên hệ & SĐT</label>
        <input v-model="form.relativeContact" type="text" placeholder="Nguyễn Văn B (Bố) - 0987654321" class="w-full px-3 py-2 border rounded-lg" />
      </div>

      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="block font-semibold text-slate-700 mb-1">Khoa điều trị</label>
          <select v-model="form.departmentCode" class="w-full px-3 py-2 border rounded-lg">
            <option value="KHOA_NOI">Khoa Nội Tổng Hợp</option>
            <option value="KHOA_NGOAI">Khoa Ngoại Tổng Hợp</option>
            <option value="KHOA_CAP_CUU">Khoa Cấp Cứu - Hồi Sức</option>
          </select>
        </div>
        <div>
          <label class="block font-semibold text-slate-700 mb-1">Chọn giường bệnh trống *</label>
          <select v-model="form.targetBedId" class="w-full px-3 py-2 border rounded-lg font-medium text-blue-700">
            <option value="" disabled>-- Chọn giường trống --</option>
            <option v-for="bed in filteredBeds" :key="bed.id" :value="bed.id">
              {{ bed.bedCode }} - {{ bed.roomName }} ({{ bed.dailyPrice.toLocaleString('vi-VN') }}đ/ngày)
            </option>
          </select>
        </div>
      </div>

      <div class="grid grid-cols-12 gap-2">
        <div class="col-span-3">
          <label class="block font-semibold text-slate-700 mb-1">Mã ICD-10 *</label>
          <input v-model="form.primaryIcd10Code" type="text" class="w-full px-2 py-2 border rounded-lg font-mono font-bold text-blue-700" />
        </div>
        <div class="col-span-9">
          <label class="block font-semibold text-slate-700 mb-1">Chẩn đoán vào viện *</label>
          <input v-model="form.initialDiagnosis" type="text" class="w-full px-3 py-2 border rounded-lg" />
        </div>
      </div>

      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="block font-semibold text-slate-700 mb-1">Bác sĩ tiếp nhận</label>
          <input v-model="form.admittingDoctorName" type="text" class="w-full px-3 py-2 border rounded-lg" />
        </div>
        <div>
          <label class="block font-semibold text-slate-700 mb-1">Tạm ứng viện phí ban đầu (VNĐ)</label>
          <input v-model.number="form.initialAdvancePayment" type="number" step="500000" class="w-full px-3 py-2 border rounded-lg font-mono font-bold text-emerald-700" />
        </div>
      </div>

      <div class="flex justify-end gap-2 pt-3 border-t">
        <button
          type="button"
          @click="emit('update:visible', false)"
          class="px-4 py-2 border border-slate-300 rounded-lg font-medium text-slate-700 hover:bg-slate-50"
        >
          Hủy bỏ
        </button>
        <button
          type="button"
          @click="handleAdmission"
          :disabled="loading"
          class="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium shadow-sm transition-colors"
        >
          <span v-if="loading">Đang xếp giường...</span>
          <span v-else>Nhập viện & Xếp giường</span>
        </button>
      </div>
    </div>
  </BaseModal>
</template>
