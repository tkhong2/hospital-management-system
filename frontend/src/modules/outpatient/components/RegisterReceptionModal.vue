<script setup lang="ts">
import { ref } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { outpatientApi } from '../api/outpatientApi'
import { PatientType } from '../types/outpatient'

const props = defineProps<{
  visible: boolean
}>()

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'registered'): void
}>()

const loading = ref(false)
const error = ref('')

const form = ref({
  patientCode: 'BN26' + Math.floor(100000 + Math.random() * 900000),
  patientName: '',
  dateOfBirth: '1990-01-01',
  gender: 'Nam',
  phoneNumber: '',
  address: '',
  healthInsuranceNumber: '',
  clinicRoomId: '8f94d455-801b-4f9e-a890-449e7b28db11',
  clinicRoomName: 'Phòng khám Nội 01',
  patientType: PatientType.OutpatientInsurance,
  reason: '',
  priority: 0
})

const clinicRooms = [
  { id: '8f94d455-801b-4f9e-a890-449e7b28db11', name: 'Phòng khám Nội 01' },
  { id: '9a12c344-772c-4e8a-b912-338e6a17ea22', name: 'Phòng khám Ngoại 02' },
  { id: '1b23d455-663d-4f7b-c823-227d5f06fa33', name: 'Phòng khám Tai Mũi Họng' },
  { id: '2c34e566-554e-4e6c-d734-116c4e95eb44', name: 'Phòng khám Nhi' }
]

function onClinicRoomChange(e: Event) {
  const target = e.target as HTMLSelectElement
  const found = clinicRooms.find(r => r.id === target.value)
  if (found) {
    form.value.clinicRoomId = found.id
    form.value.clinicRoomName = found.name
  }
}

async function handleRegister() {
  if (!form.value.patientName.trim()) {
    error.value = 'Vui lòng nhập họ tên bệnh nhân'
    return
  }

  loading.value = true
  error.value = ''
  try {
    await outpatientApi.createReception({
      patientId: 'a1b2c3d4-e5f6-7890-1234-567890abcdef',
      patientCode: form.value.patientCode,
      patientName: form.value.patientName,
      dateOfBirth: form.value.dateOfBirth,
      gender: form.value.gender,
      phoneNumber: form.value.phoneNumber,
      address: form.value.address,
      healthInsuranceNumber: form.value.healthInsuranceNumber,
      clinicRoomId: form.value.clinicRoomId,
      clinicRoomName: form.value.clinicRoomName,
      patientType: Number(form.value.patientType),
      reason: form.value.reason,
      priority: Number(form.value.priority)
    })

    emit('registered')
    emit('update:visible', false)
  } catch (err: any) {
    error.value = err.response?.data?.errors?.[0] || 'Lỗi khi tiếp đón bệnh nhân'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="visible"
    title="Tiếp đón bệnh nhân & Cấp số thứ tự khám"
    @update:model-value="emit('update:visible', $event)"
  >
    <div class="space-y-4">
      <div v-if="error" class="p-3 bg-red-50 text-red-700 text-xs rounded-lg border border-red-200">
        {{ error }}
      </div>

      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Mã bệnh nhân *</label>
          <input
            v-model="form.patientCode"
            type="text"
            class="w-full text-xs px-3 py-2 border rounded-lg bg-slate-50 border-slate-200 font-mono"
          />
        </div>
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Họ và tên *</label>
          <input
            v-model="form.patientName"
            type="text"
            placeholder="Nguyễn Văn A"
            class="w-full text-xs px-3 py-2 border rounded-lg border-slate-200 focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <div class="grid grid-cols-3 gap-3">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Ngày sinh</label>
          <input
            v-model="form.dateOfBirth"
            type="date"
            class="w-full text-xs px-3 py-2 border rounded-lg border-slate-200"
          />
        </div>
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Giới tính</label>
          <select v-model="form.gender" class="w-full text-xs px-3 py-2 border rounded-lg border-slate-200">
            <option value="Nam">Nam</option>
            <option value="Nữ">Nữ</option>
            <option value="Khác">Khác</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Số điện thoại</label>
          <input
            v-model="form.phoneNumber"
            type="tel"
            placeholder="0912345678"
            class="w-full text-xs px-3 py-2 border rounded-lg border-slate-200"
          />
        </div>
      </div>

      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Đối tượng khám</label>
          <select v-model="form.patientType" class="w-full text-xs px-3 py-2 border rounded-lg border-slate-200">
            <option :value="PatientType.OutpatientInsurance">Khám BHYT</option>
            <option :value="PatientType.ServiceFee">Khám Viện phí / Dịch vụ</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Mã thẻ BHYT (nếu có)</label>
          <input
            v-model="form.healthInsuranceNumber"
            type="text"
            placeholder="DN4010123456789"
            class="w-full text-xs px-3 py-2 border rounded-lg border-slate-200 font-mono uppercase"
          />
        </div>
      </div>

      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Chỉ định phòng khám *</label>
          <select
            :value="form.clinicRoomId"
            @change="onClinicRoomChange"
            class="w-full text-xs px-3 py-2 border rounded-lg border-slate-200 font-medium"
          >
            <option v-for="r in clinicRooms" :key="r.id" :value="r.id">{{ r.name }}</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Mức độ ưu tiên</label>
          <select v-model="form.priority" class="w-full text-xs px-3 py-2 border rounded-lg border-slate-200">
            <option :value="0">Bình thường (Thứ tự tự động)</option>
            <option :value="1">Ưu tiên (Trẻ em < 6 tuổi / Người già >= 75 / Cấp cứu nhẹ)</option>
          </select>
        </div>
      </div>

      <div>
        <label class="block text-xs font-semibold text-slate-700 mb-1">Lý do đến khám / Triệu chứng ban đầu</label>
        <textarea
          v-model="form.reason"
          rows="2"
          placeholder="Đau ngực, khó thở, sốt cao..."
          class="w-full text-xs px-3 py-2 border rounded-lg border-slate-200"
        ></textarea>
      </div>

      <div class="flex justify-end gap-2 pt-3 border-t">
        <button
          type="button"
          @click="emit('update:visible', false)"
          class="px-4 py-2 border border-slate-300 rounded-lg text-xs font-medium text-slate-700 hover:bg-slate-50"
        >
          Hủy bỏ
        </button>
        <button
          type="button"
          @click="handleRegister"
          :disabled="loading"
          class="px-5 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-medium shadow-sm transition-colors flex items-center gap-1.5"
        >
          <span v-if="loading">Đang cấp số...</span>
          <span v-else>Cấp số & Vào hàng chờ</span>
        </button>
      </div>
    </div>
  </BaseModal>
</template>
