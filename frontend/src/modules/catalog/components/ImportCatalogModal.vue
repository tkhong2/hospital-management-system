<script setup lang="ts">
import { ref } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { catalogApi } from '../api/catalogApi'
import { FileUp, CheckCircle2, AlertCircle, Loader2, Copy } from 'lucide-vue-next'

const props = defineProps<{
  modelValue: boolean
  targetType: 'services' | 'icd10'
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'imported'): void
}>()

const rawInput = ref('')
const isSubmitting = ref(false)
const errorMessage = ref('')
const successStats = ref<{ totalProcessed: number; successCount: number; failureCount: number } | null>(null)

const sampleServiceJson = `[
  {
    "code": "KB.NOI",
    "name": "Khám Nội tổng quát",
    "serviceType": 1,
    "unitName": "Lần",
    "price": 120000,
    "insurancePrice": 85000
  },
  {
    "code": "XN.CTM",
    "name": "Tổng phân tích tế bào máu ngoại vi",
    "serviceType": 2,
    "unitName": "Lần",
    "price": 65000,
    "insurancePrice": 52000
  }
]`

const sampleIcd10Json = `[
  {
    "code": "I10",
    "nameVi": "Bệnh tăng huyết áp vô căn (nguyên phát)",
    "nameEn": "Essential (primary) hypertension",
    "chapterCode": "IX",
    "chapterName": "Bệnh hệ tuần hoàn"
  },
  {
    "code": "E11",
    "nameVi": "Bệnh đái tháo đường không phụ thuộc insulin",
    "nameEn": "Non-insulin-dependent diabetes mellitus",
    "chapterCode": "IV",
    "chapterName": "Bệnh nội tiết, dinh dưỡng và chuyển hóa"
  }
]`

const fillSample = () => {
  rawInput.value = props.targetType === 'services' ? sampleServiceJson : sampleIcd10Json
  errorMessage.value = ''
  successStats.value = null
}

const handleFileUpload = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  const reader = new FileReader()
  reader.onload = (e) => {
    rawInput.value = (e.target?.result as string) || ''
  }
  reader.readAsText(file)
}

const handleImport = async () => {
  if (!rawInput.value.trim()) {
    errorMessage.value = 'Vui lòng nhập hoặc dán nội dung dữ liệu cần import'
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''
  successStats.value = null

  try {
    let parsed: any[]
    try {
      parsed = JSON.parse(rawInput.value.trim())
      if (!Array.isArray(parsed)) {
        throw new Error('Dữ liệu JSON phải là một mảng danh sách các bản ghi')
      }
    } catch (e: any) {
      errorMessage.value = `Lỗi định dạng JSON: ${e.message}`
      isSubmitting.value = false
      return
    }

    if (props.targetType === 'services') {
      const res = await catalogApi.importMedicalServices(parsed)
      if (res.success && res.data) {
        successStats.value = res.data
        emit('imported')
      } else {
        errorMessage.value = res.errors?.[0] || 'Lỗi khi import danh mục dịch vụ'
      }
    } else {
      const res = await catalogApi.importIcd10(parsed)
      if (res.success && res.data) {
        successStats.value = res.data
        emit('imported')
      } else {
        errorMessage.value = res.errors?.[0] || 'Lỗi khi import danh mục ICD-10'
      }
    }
  } catch (err: any) {
    errorMessage.value = err?.message || 'Có lỗi xảy ra trong quá trình nạp dữ liệu'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event)"
    :title="targetType === 'services' ? 'Import Hàng Loạt Dịch Vụ Kỹ Thuật' : 'Import Danh Mục Mã Bệnh ICD-10'"
    subtitle="Nạp nhanh hàng nghìn bản ghi danh mục qua định dạng JSON hoặc tệp dữ liệu chuẩn"
    max-width="3xl"
  >
    <!-- Success Banner -->
    <div
      v-if="successStats"
      class="mb-4 p-4 rounded-xl bg-emerald-50 border border-emerald-200 text-emerald-900 text-xs flex items-center justify-between"
    >
      <div class="flex items-center gap-2.5">
        <CheckCircle2 class="w-5 h-5 text-emerald-600 shrink-0" />
        <div>
          <span class="font-bold text-emerald-800">Import thành công hoàn tất!</span>
          <p class="text-emerald-700 text-[11px] mt-0.5">
            Tổng xử lý: <strong>{{ successStats.totalProcessed }}</strong> bản ghi | Thành công: <strong>{{ successStats.successCount }}</strong> | Bỏ qua: <strong>{{ successStats.failureCount }}</strong>
          </p>
        </div>
      </div>
      <button
        @click="emit('update:modelValue', false)"
        class="px-3 py-1 bg-emerald-600 text-white rounded-lg text-xs font-bold hover:bg-emerald-700"
      >
        Đóng
      </button>
    </div>

    <!-- Error Banner -->
    <div
      v-if="errorMessage"
      class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs font-medium"
    >
      {{ errorMessage }}
    </div>

    <div class="space-y-4">
      <div class="flex items-center justify-between">
        <label class="block text-xs font-semibold text-slate-700">
          Nội dung dữ liệu JSON danh mục
        </label>
        <div class="flex items-center gap-2">
          <button
            type="button"
            @click="fillSample"
            class="inline-flex items-center gap-1 text-xs text-primary hover:text-primary-hover font-semibold"
          >
            <Copy class="w-3.5 h-3.5" />
            <span>Nạp mẫu JSON</span>
          </button>
          <span class="text-slate-300">|</span>
          <label class="inline-flex items-center gap-1 text-xs text-slate-600 hover:text-slate-800 font-medium cursor-pointer">
            <FileUp class="w-3.5 h-3.5" />
            <span>Tải tệp JSON/TXT</span>
            <input type="file" accept=".json,.txt,.csv" class="hidden" @change="handleFileUpload" />
          </label>
        </div>
      </div>

      <textarea
        v-model="rawInput"
        rows="12"
        placeholder="Dán mảng JSON vào đây..."
        class="w-full p-3 text-xs bg-slate-900 text-slate-100 font-mono rounded-xl focus:ring-2 focus:ring-primary focus:outline-none leading-relaxed"
      />
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
        @click="handleImport"
        :disabled="isSubmitting || !rawInput.trim()"
        type="button"
        class="inline-flex items-center gap-1.5 px-4 py-2 text-xs font-bold text-white bg-primary rounded-xl hover:bg-primary-hover transition-colors shadow-sm disabled:opacity-50"
      >
        <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
        <FileUp v-else class="w-4 h-4" />
        <span>Thực hiện Import</span>
      </button>
    </template>
  </BaseModal>
</template>

