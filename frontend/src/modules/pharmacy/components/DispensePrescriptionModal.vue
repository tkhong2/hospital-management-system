<script setup lang="ts">
import { ref } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { pharmacyApi } from '../api/pharmacyApi'
import type { PrescriptionOrder } from '../types/pharmacy'
import { CheckCircle2, AlertCircle, Loader2, Pill } from 'lucide-vue-next'

const props = defineProps<{
  modelValue: boolean
  prescription: PrescriptionOrder | null
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'dispensed'): void
}>()

const isSubmitting = ref(false)
const errorMessage = ref('')
const dispensedBy = ref('DS. Đặng Minh Quân')

const handleDispense = async () => {
  if (!props.prescription) return

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const res = await pharmacyApi.dispensePrescription(
      props.prescription.id,
      props.prescription.warehouseId,
      dispensedBy.value
    )

    if (res.success) {
      emit('dispensed')
      emit('update:modelValue', false)
    } else {
      errorMessage.value = res.errors?.[0] || 'Lỗi khi cấp phát đơn thuốc'
    }
  } catch (err: any) {
    errorMessage.value = err?.message || 'Có lỗi xảy ra khi thực hiện cấp phát'
  } finally {
    isSubmitting.value = false
  }
}

const formatCurrency = (val: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event)"
    title="Duyệt & Cấp Phát Đơn Thuốc (FEFO)"
    :subtitle="`Mã đơn: ${prescription?.prescriptionCode} - Bệnh nhân: ${prescription?.patientName} (${prescription?.patientCode})`"
    max-width="3xl"
  >
    <div
      v-if="errorMessage"
      class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs font-medium"
    >
      {{ errorMessage }}
    </div>

    <div v-if="prescription" class="space-y-4">
      <!-- Patient & Doctor Info -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-3 bg-slate-50 p-3.5 rounded-xl border border-slate-200 text-xs">
        <div>
          <span class="text-slate-400 block text-[11px]">Bệnh nhân</span>
          <span class="font-bold text-slate-800 uppercase">{{ prescription.patientName }}</span>
        </div>
        <div>
          <span class="text-slate-400 block text-[11px]">Mã BN</span>
          <span class="font-mono font-semibold text-primary">{{ prescription.patientCode }}</span>
        </div>
        <div>
          <span class="text-slate-400 block text-[11px]">Bác sĩ kê đơn</span>
          <span class="font-semibold text-slate-800">{{ prescription.doctorName || '—' }}</span>
        </div>
        <div>
          <span class="text-slate-400 block text-[11px]">Kho xuất</span>
          <span class="font-semibold text-indigo-700">{{ prescription.warehouseName }}</span>
        </div>
        <div class="col-span-2 md:col-span-4 border-t border-slate-200/60 pt-2">
          <span class="text-slate-400 text-[11px]">Chẩn đoán: </span>
          <span class="font-medium text-slate-700">{{ prescription.diagnosis || 'Không ghi rõ' }}</span>
        </div>
      </div>

      <!-- Prescribed Medicines List -->
      <div class="border border-slate-200 rounded-xl overflow-hidden">
        <div class="bg-slate-50/80 px-4 py-2 border-b border-slate-200 flex items-center justify-between text-xs font-semibold text-slate-700">
          <span>Chi tiết thuốc & hàm lượng</span>
          <span>Số lượng yêu cầu</span>
        </div>
        <div class="divide-y divide-slate-100">
          <div
            v-for="d in prescription.details"
            :key="d.id"
            class="p-3.5 flex items-center justify-between gap-4 text-xs hover:bg-slate-50/50"
          >
            <div class="space-y-0.5">
              <div class="flex items-center gap-2">
                <span class="font-mono font-bold text-primary">[{{ d.medicineCode }}]</span>
                <span class="font-bold text-slate-800">{{ d.medicineName }}</span>
              </div>
              <p v-if="d.usageInstructions" class="text-[11px] text-slate-500 italic">
                HDSD: {{ d.usageInstructions }}
              </p>
              <div v-if="d.allocatedBatchNumber" class="text-[11px] text-emerald-700 font-medium">
                Đã gán lô: <strong>{{ d.allocatedBatchNumber }}</strong> (HSD: {{ d.allocatedExpiryDate?.slice(0, 10) }})
              </div>
            </div>

            <div class="text-right">
              <div class="font-bold text-sm text-slate-900">
                {{ d.quantityRequested }} <span class="text-xs font-normal text-slate-500">{{ d.unitName }}</span>
              </div>
              <div class="text-[11px] text-slate-400">
                Đơn giá: {{ formatCurrency(d.unitPrice) }}
              </div>
            </div>
          </div>
        </div>

        <div class="bg-slate-50 p-3 px-4 border-t border-slate-200 flex items-center justify-between text-xs">
          <span class="text-slate-600 font-semibold">Tổng tiền thanh toán đơn:</span>
          <span class="text-sm font-bold text-emerald-700">{{ formatCurrency(prescription.totalAmount) }}</span>
        </div>
      </div>

      <!-- FEFO Policy Notice -->
      <div class="p-3 rounded-xl bg-blue-50 border border-blue-200 text-blue-900 text-xs flex items-start gap-2.5">
        <AlertCircle class="w-4 h-4 text-blue-600 shrink-0 mt-0.5" />
        <div class="text-[11px] leading-relaxed">
          <strong>Quy tắc xuất FEFO tự động:</strong> Hệ thống tự động quét và trừ số lượng từ các lô thuốc có hạn sử dụng gần nhất trước.
          Mỗi dòng thuốc xuất sẽ được tự động ghi nhận vào <strong>Sổ Thẻ Kho (Stock Ledger)</strong> để đối soát kiểm kê.
        </div>
      </div>

      <div>
        <label class="block text-xs font-semibold text-slate-700 mb-1">Dược sĩ phát thuốc</label>
        <input
          v-model="dispensedBy"
          type="text"
          class="w-full px-3.5 py-2 text-xs bg-white border border-slate-200 rounded-xl focus:ring-1 focus:ring-primary text-slate-800 font-medium"
        />
      </div>
    </div>

    <template #footer="{ close }">
      <button
        @click="close"
        type="button"
        class="px-4 py-2 text-xs font-semibold text-slate-600 bg-white border border-slate-200 rounded-xl hover:bg-slate-100 transition-colors shadow-xs"
      >
        Đóng
      </button>
      <button
        v-if="prescription?.status === 1 || prescription?.status === 2"
        @click="handleDispense"
        :disabled="isSubmitting"
        type="button"
        class="inline-flex items-center gap-1.5 px-4 py-2 text-xs font-bold text-white bg-emerald-600 rounded-xl hover:bg-emerald-700 transition-colors shadow-sm disabled:opacity-50"
      >
        <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
        <CheckCircle2 v-else class="w-4 h-4" />
        <span>Xác nhận Cấp phát & Trừ tồn kho</span>
      </button>
    </template>
  </BaseModal>
</template>

