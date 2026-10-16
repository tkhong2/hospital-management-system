<script setup lang="ts">
import { ref } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { patientApi } from '../api/patientApi'
import type { PatientListItem } from '../types/patient'
import { GitMerge, ArrowRight, AlertCircle, Loader2 } from 'lucide-vue-next'

const props = defineProps<{
  modelValue: boolean
  targetPatient: PatientListItem | null
  allPatients: PatientListItem[]
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'merged'): void
}>()

const selectedSourceId = ref('')
const mergeReason = ref('Phát hiện trùng lặp hồ sơ khám bệnh qua số CCCD / BHYT')
const isSubmitting = ref(false)
const errorMessage = ref('')

const handleMerge = async () => {
  if (!props.targetPatient || !selectedSourceId.value) {
    errorMessage.value = 'Vui lòng chọn hồ sơ phụ cần gộp'
    return
  }

  if (selectedSourceId.value === props.targetPatient.id) {
    errorMessage.value = 'Không thể gộp chính hồ sơ này vào chính nó'
    return
  }

  errorMessage.value = ''
  isSubmitting.value = true

  try {
    const res = await patientApi.mergePatients({
      sourcePatientId: selectedSourceId.value,
      targetPatientId: props.targetPatient.id,
      mergeReason: mergeReason.value.trim()
    })

    if (res.success) {
      emit('merged')
      emit('update:modelValue', false)
    } else {
      errorMessage.value = res.errors?.[0] || 'Lỗi khi gộp hồ sơ'
    }
  } catch (err: any) {
    errorMessage.value = err?.message || err?.errors?.[0] || 'Không thể thực hiện gộp hồ sơ'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event)"
    title="Hợp nhất Hồ sơ Bệnh nhân (MPI Record Merge)"
    subtitle="Gộp các hồ sơ trùng lặp vào một hồ sơ duy nhất để đảm bảo tính toàn vẹn dữ liệu lâm sàng"
    max-width="2xl"
  >
    <div
      v-if="errorMessage"
      class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs font-medium"
    >
      {{ errorMessage }}
    </div>

    <div class="space-y-4">
      <!-- Target Patient (Retained) -->
      <div class="p-4 rounded-xl bg-emerald-50/70 border border-emerald-200">
        <div class="flex items-center gap-2 mb-2">
          <span class="px-2 py-0.5 rounded text-[11px] font-bold bg-emerald-600 text-white uppercase tracking-wider">
            Hồ sơ Chính (Giữ lại)
          </span>
          <span class="text-xs font-semibold text-emerald-800">
            [{{ targetPatient?.patientCode }}] {{ targetPatient?.fullName }}
          </span>
        </div>
        <div class="grid grid-cols-2 gap-2 text-xs text-emerald-900">
          <div><span class="text-emerald-700">Ngày sinh:</span> {{ targetPatient?.dateOfBirth?.slice(0, 10) }}</div>
          <div><span class="text-emerald-700">SĐT:</span> {{ targetPatient?.phoneNumber || 'Chưa có' }}</div>
          <div><span class="text-emerald-700">CCCD:</span> {{ targetPatient?.identityCardNumber || 'Chưa có' }}</div>
          <div><span class="text-emerald-700">BHYT:</span> {{ targetPatient?.healthInsuranceCardNumber || 'Chưa có' }}</div>
        </div>
      </div>

      <!-- Source Patient Selection -->
      <div>
        <label class="block text-xs font-semibold text-slate-700 mb-1">
          Chọn Hồ sơ Phụ (Cần gộp vào hồ sơ chính) <span class="text-rose-500">*</span>
        </label>
        <select
          v-model="selectedSourceId"
          class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
        >
          <option value="" disabled>-- Chọn hồ sơ trùng lặp cần gộp --</option>
          <option
            v-for="p in allPatients.filter(x => x.id !== targetPatient?.id && !x.isMerged)"
            :key="p.id"
            :value="p.id"
          >
            [{{ p.patientCode }}] {{ p.fullName }} - NS: {{ p.dateOfBirth?.slice(0, 10) }} (CCCD: {{ p.identityCardNumber || 'N/A' }})
          </option>
        </select>
      </div>

      <!-- Reason -->
      <div>
        <label class="block text-xs font-semibold text-slate-700 mb-1">Lý do gộp hồ sơ</label>
        <input
          v-model="mergeReason"
          type="text"
          placeholder="Nhập lý do hợp nhất hồ sơ..."
          class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
        />
      </div>

      <!-- Info note -->
      <div class="p-3 rounded-xl bg-blue-50 border border-blue-200 text-blue-800 text-xs flex items-start gap-2.5">
        <AlertCircle class="w-4 h-4 text-blue-600 shrink-0 mt-0.5" />
        <div class="space-y-1 text-[11px] leading-relaxed">
          <p><strong>Cơ chế bảo toàn MPI:</strong> Hồ sơ phụ sẽ không bị xóa vật lý mà được đánh dấu <code class="bg-blue-100 px-1 py-0.5 rounded">IsMerged = true</code> và lưu trữ bản ghi snapshot trong lịch sử kiểm toán (Audit History).</p>
          <p>Mọi thông tin còn thiếu từ hồ sơ phụ (nhóm máu, dị ứng, BHYT) sẽ được tự động tích hợp sang hồ sơ chính.</p>
        </div>
      </div>
    </div>

    <template #footer="{ close }">
      <button
        @click="close"
        type="button"
        class="px-4 py-2 text-xs font-semibold text-slate-600 bg-white border border-slate-200 rounded-xl hover:bg-slate-100 transition-colors shadow-xs"
      >
        Hủy bỏ
      </button>
      <button
        @click="handleMerge"
        :disabled="isSubmitting || !selectedSourceId"
        type="button"
        class="inline-flex items-center gap-1.5 px-4 py-2 text-xs font-bold text-white bg-indigo-600 rounded-xl hover:bg-indigo-700 transition-colors shadow-sm disabled:opacity-50"
      >
        <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
        <GitMerge v-else class="w-4 h-4" />
        <span>Xác nhận Gộp hồ sơ</span>
      </button>
    </template>
  </BaseModal>
</template>

