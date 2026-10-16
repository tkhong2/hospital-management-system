<script setup lang="ts">
import { ref, reactive, watch, computed } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { Gender, BloodType, type Patient, type CreatePatientDto, type UpdatePatientDto, type DuplicateCandidate } from '../types/patient'
import { patientApi } from '../api/patientApi'
import { AlertTriangle, UserCheck, Loader2 } from 'lucide-vue-next'

const props = defineProps<{
  modelValue: boolean
  patientToEdit?: Patient | null
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'saved'): void
}>()

const isSubmitting = ref(false)
const errorMessage = ref('')
const duplicateCandidates = ref<DuplicateCandidate[]>([])
const isCheckingDuplicate = ref(false)

const form = reactive<{
  fullName: string
  dateOfBirth: string
  gender: Gender
  identityCardNumber: string
  healthInsuranceCardNumber: string
  phoneNumber: string
  email: string
  address: string
  provinceName: string
  districtName: string
  wardName: string
  bloodType: BloodType
  allergies: string
  medicalHistory: string
}>({
  fullName: '',
  dateOfBirth: '',
  gender: Gender.Male,
  identityCardNumber: '',
  healthInsuranceCardNumber: '',
  phoneNumber: '',
  email: '',
  address: '',
  provinceName: '',
  districtName: '',
  wardName: '',
  bloodType: BloodType.Unknown,
  allergies: '',
  medicalHistory: ''
})

const isEditMode = computed(() => !!props.patientToEdit)

watch(
  () => props.patientToEdit,
  (val) => {
    if (val) {
      form.fullName = val.fullName || ''
      form.dateOfBirth = val.dateOfBirth ? val.dateOfBirth.slice(0, 10) : ''
      form.gender = val.gender ?? Gender.Male
      form.identityCardNumber = val.identityCardNumber || ''
      form.healthInsuranceCardNumber = val.healthInsuranceCardNumber || ''
      form.phoneNumber = val.phoneNumber || ''
      form.email = val.email || ''
      form.address = val.address || ''
      form.provinceName = val.provinceName || ''
      form.districtName = val.districtName || ''
      form.wardName = val.wardName || ''
      form.bloodType = val.bloodType ?? BloodType.Unknown
      form.allergies = val.allergies || ''
      form.medicalHistory = val.medicalHistory || ''
      duplicateCandidates.value = []
    } else {
      resetForm()
    }
  },
  { immediate: true }
)

function resetForm() {
  form.fullName = ''
  form.dateOfBirth = ''
  form.gender = Gender.Male
  form.identityCardNumber = ''
  form.healthInsuranceCardNumber = ''
  form.phoneNumber = ''
  form.email = ''
  form.address = ''
  form.provinceName = 'Hà Nội'
  form.districtName = 'Ba Đình'
  form.wardName = ''
  form.bloodType = BloodType.Unknown
  form.allergies = ''
  form.medicalHistory = ''
  duplicateCandidates.value = []
  errorMessage.value = ''
}

// Real-time duplicate check with debounce
let checkTimeout: ReturnType<typeof setTimeout> | null = null
const triggerDuplicateCheck = () => {
  if (checkTimeout) clearTimeout(checkTimeout)
  checkTimeout = setTimeout(async () => {
    if (!form.identityCardNumber && !form.healthInsuranceCardNumber && (!form.fullName || !form.dateOfBirth)) {
      duplicateCandidates.value = []
      return
    }

    try {
      isCheckingDuplicate.value = true
      const res = await patientApi.checkDuplicates({
        identityCardNumber: form.identityCardNumber || undefined,
        healthInsuranceCardNumber: form.healthInsuranceCardNumber || undefined,
        fullName: form.fullName || undefined,
        dateOfBirth: form.dateOfBirth || undefined,
        phoneNumber: form.phoneNumber || undefined,
        excludePatientId: props.patientToEdit?.id
      })
      if (res.success && res.data) {
        duplicateCandidates.value = res.data
      } else {
        duplicateCandidates.value = []
      }
    } catch {
      duplicateCandidates.value = []
    } finally {
      isCheckingDuplicate.value = false
    }
  }, 500)
}

const handleSubmit = async () => {
  if (!form.fullName.trim()) {
    errorMessage.value = 'Họ và tên không được để trống'
    return
  }
  if (!form.dateOfBirth) {
    errorMessage.value = 'Ngày sinh không được để trống'
    return
  }

  errorMessage.value = ''
  isSubmitting.value = true

  try {
    if (isEditMode.value && props.patientToEdit) {
      const payload: UpdatePatientDto = {
        fullName: form.fullName.trim(),
        dateOfBirth: new Date(form.dateOfBirth).toISOString(),
        gender: Number(form.gender),
        identityCardNumber: form.identityCardNumber.trim() || undefined,
        healthInsuranceCardNumber: form.healthInsuranceCardNumber.trim() || undefined,
        phoneNumber: form.phoneNumber.trim() || undefined,
        email: form.email.trim() || undefined,
        address: form.address.trim() || undefined,
        provinceName: form.provinceName.trim() || undefined,
        districtName: form.districtName.trim() || undefined,
        wardName: form.wardName.trim() || undefined,
        bloodType: Number(form.bloodType),
        allergies: form.allergies.trim() || undefined,
        medicalHistory: form.medicalHistory.trim() || undefined
      }

      const res = await patientApi.updatePatient(props.patientToEdit.id, payload)
      if (res.success) {
        emit('saved')
        emit('update:modelValue', false)
      } else {
        errorMessage.value = res.errors?.[0] || 'Lỗi khi cập nhật bệnh nhân'
      }
    } else {
      const payload: CreatePatientDto = {
        fullName: form.fullName.trim(),
        dateOfBirth: new Date(form.dateOfBirth).toISOString(),
        gender: Number(form.gender),
        identityCardNumber: form.identityCardNumber.trim() || undefined,
        healthInsuranceCardNumber: form.healthInsuranceCardNumber.trim() || undefined,
        phoneNumber: form.phoneNumber.trim() || undefined,
        email: form.email.trim() || undefined,
        address: form.address.trim() || undefined,
        provinceName: form.provinceName.trim() || undefined,
        districtName: form.districtName.trim() || undefined,
        wardName: form.wardName.trim() || undefined,
        bloodType: Number(form.bloodType),
        allergies: form.allergies.trim() || undefined,
        medicalHistory: form.medicalHistory.trim() || undefined
      }

      const res = await patientApi.createPatient(payload)
      if (res.success) {
        emit('saved')
        emit('update:modelValue', false)
      } else {
        errorMessage.value = res.errors?.[0] || 'Lỗi khi tạo bệnh nhân'
      }
    }
  } catch (err: any) {
    errorMessage.value = err?.message || err?.errors?.[0] || 'Không thể lưu hồ sơ bệnh nhân'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event)"
    :title="isEditMode ? 'Chỉnh sửa Hồ sơ Bệnh nhân' : 'Tiếp đón & Tạo mới Hồ sơ Bệnh nhân (MPI)'"
    :subtitle="isEditMode ? `Mã bệnh nhân: ${patientToEdit?.patientCode}` : 'Hệ thống tự động sinh mã BN định danh duy nhất và phát hiện trùng lặp'"
    max-width="3xl"
  >
    <!-- Duplicate Warning Banner -->
    <div
      v-if="duplicateCandidates.length > 0"
      class="mb-5 p-4 rounded-xl bg-amber-50 border border-amber-200 text-amber-900 text-xs flex items-start gap-3"
    >
      <AlertTriangle class="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
      <div class="flex-1">
        <p class="font-bold text-amber-800">Cảnh báo: Phát hiện {{ duplicateCandidates.length }} hồ sơ có khả năng trùng lặp!</p>
        <div class="mt-2 space-y-1.5 max-h-32 overflow-y-auto pr-1">
          <div
            v-for="c in duplicateCandidates"
            :key="c.id"
            class="p-2 bg-white/80 rounded-lg border border-amber-200/80 flex items-center justify-between"
          >
            <div>
              <span class="font-semibold text-slate-800">[{{ c.patientCode }}] {{ c.fullName }}</span>
              <span class="text-slate-500 ml-2">({{ c.dateOfBirth?.slice(0, 10) }})</span>
              <div class="text-[11px] text-amber-700 mt-0.5">
                Trùng khớp: {{ c.duplicateReasons.join(', ') }}
              </div>
            </div>
            <span class="px-2 py-0.5 rounded text-[10px] font-bold bg-amber-200 text-amber-800">
              Đã tồn tại
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Error Alert -->
    <div
      v-if="errorMessage"
      class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs font-medium"
    >
      {{ errorMessage }}
    </div>

    <!-- Form Fields -->
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Full Name -->
        <div class="md:col-span-2">
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Họ và tên <span class="text-rose-500">*</span>
          </label>
          <input
            v-model="form.fullName"
            type="text"
            required
            placeholder="NGUYỄN VĂN A"
            @input="triggerDuplicateCheck"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary uppercase font-medium text-slate-800"
          />
        </div>

        <!-- Date of birth -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Ngày sinh <span class="text-rose-500">*</span>
          </label>
          <input
            v-model="form.dateOfBirth"
            type="date"
            required
            @change="triggerDuplicateCheck"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Gender -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Giới tính</label>
          <select
            v-model="form.gender"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          >
            <option :value="Gender.Male">Nam</option>
            <option :value="Gender.Female">Nữ</option>
            <option :value="Gender.Other">Khác</option>
          </select>
        </div>

        <!-- CCCD / Identity Card -->
        <div>
          <div class="flex items-center justify-between mb-1">
            <label class="block text-xs font-semibold text-slate-700">Số CCCD / Định danh</label>
            <span v-if="isCheckingDuplicate" class="text-[10px] text-slate-400 flex items-center gap-1">
              <Loader2 class="w-3 h-3 animate-spin" /> Kiểm tra trùng...
            </span>
          </div>
          <input
            v-model="form.identityCardNumber"
            type="text"
            maxlength="12"
            placeholder="12 chữ số CCCD"
            @input="triggerDuplicateCheck"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-mono"
          />
        </div>

        <!-- BHYT Card -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Mã thẻ BHYT</label>
          <input
            v-model="form.healthInsuranceCardNumber"
            type="text"
            maxlength="15"
            placeholder="15 ký tự (VD: DN401...)"
            @input="triggerDuplicateCheck"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 uppercase font-mono"
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Phone Number -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Số điện thoại</label>
          <input
            v-model="form.phoneNumber"
            type="tel"
            placeholder="0912345678"
            @input="triggerDuplicateCheck"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>

        <!-- Email -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Email</label>
          <input
            v-model="form.email"
            type="email"
            placeholder="benhnhan@email.com"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>

        <!-- Blood Type -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Nhóm máu</label>
          <select
            v-model="form.bloodType"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          >
            <option :value="BloodType.Unknown">Chưa xác định</option>
            <option :value="BloodType.A_Positive">A+</option>
            <option :value="BloodType.A_Negative">A-</option>
            <option :value="BloodType.B_Positive">B+</option>
            <option :value="BloodType.B_Negative">B-</option>
            <option :value="BloodType.O_Positive">O+</option>
            <option :value="BloodType.O_Negative">O-</option>
            <option :value="BloodType.AB_Positive">AB+</option>
            <option :value="BloodType.AB_Negative">AB-</option>
          </select>
        </div>
      </div>

      <!-- Administrative Address -->
      <div class="p-3.5 rounded-xl bg-slate-50/70 border border-slate-200/70 space-y-3">
        <span class="text-xs font-bold text-slate-700 uppercase tracking-wider block">Địa chỉ thường trú / liên hệ</span>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-3">
          <div>
            <label class="block text-[11px] font-medium text-slate-600 mb-1">Tỉnh / Thành phố</label>
            <input
              v-model="form.provinceName"
              type="text"
              placeholder="VD: Hà Nội, TP.HCM..."
              class="w-full px-3 py-1.5 text-xs bg-white border border-slate-200 rounded-lg focus:ring-1 focus:ring-primary text-slate-800"
            />
          </div>
          <div>
            <label class="block text-[11px] font-medium text-slate-600 mb-1">Quận / Huyện</label>
            <input
              v-model="form.districtName"
              type="text"
              placeholder="VD: Ba Đình, Quận 1..."
              class="w-full px-3 py-1.5 text-xs bg-white border border-slate-200 rounded-lg focus:ring-1 focus:ring-primary text-slate-800"
            />
          </div>
          <div>
            <label class="block text-[11px] font-medium text-slate-600 mb-1">Phường / Xã</label>
            <input
              v-model="form.wardName"
              type="text"
              placeholder="VD: Phường Điện Biên..."
              class="w-full px-3 py-1.5 text-xs bg-white border border-slate-200 rounded-lg focus:ring-1 focus:ring-primary text-slate-800"
            />
          </div>
        </div>
        <div>
          <label class="block text-[11px] font-medium text-slate-600 mb-1">Địa chỉ chi tiết (Số nhà, tên đường, thôn xóm)</label>
          <input
            v-model="form.address"
            type="text"
            placeholder="Số 123 Đường Hoàng Hoa Thám"
            class="w-full px-3 py-1.5 text-xs bg-white border border-slate-200 rounded-lg focus:ring-1 focus:ring-primary text-slate-800"
          />
        </div>
      </div>

      <!-- Clinical Background -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Tiền sử dị ứng (Thuốc, thức ăn, phấn hoa...)</label>
          <textarea
            v-model="form.allergies"
            rows="2"
            placeholder="Dị ứng Penicillin, Paracetamol..."
            class="w-full px-3 py-2 text-xs bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Tiền sử bệnh lý bản thân & gia đình</label>
          <textarea
            v-model="form.medicalHistory"
            rows="2"
            placeholder="Tăng huyết áp, Đái tháo đường Type 2..."
            class="w-full px-3 py-2 text-xs bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>
      </div>
    </form>

    <template #footer="{ close }">
      <button
        @click="close"
        type="button"
        class="px-4 py-2 text-xs font-semibold text-slate-600 bg-white border border-slate-200 rounded-xl hover:bg-slate-100 transition-colors shadow-xs"
      >
        Hủy bỏ
      </button>
      <button
        @click="handleSubmit"
        :disabled="isSubmitting"
        type="button"
        class="inline-flex items-center gap-1.5 px-4 py-2 text-xs font-bold text-white bg-primary rounded-xl hover:bg-primary-hover transition-colors shadow-sm disabled:opacity-50"
      >
        <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
        <UserCheck v-else class="w-4 h-4" />
        <span>{{ isEditMode ? 'Lưu thay đổi' : 'Xác nhận tạo hồ sơ' }}</span>
      </button>
    </template>
  </BaseModal>
</template>

