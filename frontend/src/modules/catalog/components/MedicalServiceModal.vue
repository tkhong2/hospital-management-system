<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { catalogApi } from '../api/catalogApi'
import { ServiceType, type Department, type CreateMedicalServiceDto } from '../types/catalog'
import { PlusCircle, Loader2 } from 'lucide-vue-next'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'saved'): void
}>()

const isSubmitting = ref(false)
const errorMessage = ref('')
const departments = ref<Department[]>([])

const form = reactive<CreateMedicalServiceDto>({
  code: '',
  name: '',
  serviceType: ServiceType.Examination,
  departmentId: undefined,
  unitName: 'Lần',
  price: 100000,
  insurancePrice: 80000,
  description: '',
  isActive: true
})

onMounted(async () => {
  try {
    const res = await catalogApi.getDepartments()
    if (res.success && res.data) {
      departments.value = res.data
    }
  } catch (err) {
    console.error('Error fetching departments:', err)
  }
})

const resetForm = () => {
  form.code = ''
  form.name = ''
  form.serviceType = ServiceType.Examination
  form.departmentId = undefined
  form.unitName = 'Lần'
  form.price = 100000
  form.insurancePrice = 80000
  form.description = ''
  form.isActive = true
  errorMessage.value = ''
}

const handleSubmit = async () => {
  if (!form.code.trim()) {
    errorMessage.value = 'Mã dịch vụ không được để trống'
    return
  }
  if (!form.name.trim()) {
    errorMessage.value = 'Tên dịch vụ không được để trống'
    return
  }
  if (form.price < 0) {
    errorMessage.value = 'Đơn giá dịch vụ phải lớn hơn hoặc bằng 0'
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const res = await catalogApi.createMedicalService({
      code: form.code.trim().toUpperCase(),
      name: form.name.trim(),
      serviceType: Number(form.serviceType),
      departmentId: form.departmentId || undefined,
      unitName: form.unitName?.trim() || 'Lần',
      price: Number(form.price),
      insurancePrice: form.insurancePrice ? Number(form.insurancePrice) : undefined,
      description: form.description?.trim() || undefined,
      isActive: form.isActive
    })

    if (res.success) {
      resetForm()
      emit('saved')
      emit('update:modelValue', false)
    } else {
      errorMessage.value = res.errors?.[0] || 'Lỗi khi tạo dịch vụ kỹ thuật'
    }
  } catch (err: any) {
    errorMessage.value = err?.message || err?.errors?.[0] || 'Không thể tạo dịch vụ'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event)"
    title="Thêm Mới Dịch Vụ Kỹ Thuật"
    subtitle="Cấu hình danh mục dịch vụ khám, xét nghiệm, CĐHA và bảng giá theo quy định"
    max-width="2xl"
  >
    <div
      v-if="errorMessage"
      class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs font-medium"
    >
      {{ errorMessage }}
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Service Code -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Mã dịch vụ <span class="text-rose-500">*</span>
          </label>
          <input
            v-model="form.code"
            type="text"
            required
            placeholder="VD: KB.NOI, XN.CTM..."
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary uppercase font-mono font-medium text-slate-800"
          />
        </div>

        <!-- Service Type -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Nhóm kỹ thuật <span class="text-rose-500">*</span>
          </label>
          <select
            v-model="form.serviceType"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-medium"
          >
            <option :value="ServiceType.Examination">Khám bệnh</option>
            <option :value="ServiceType.Laboratory">Xét nghiệm</option>
            <option :value="ServiceType.Imaging">Chẩn đoán hình ảnh</option>
            <option :value="ServiceType.FunctionalProbe">Thăm dò chức năng</option>
            <option :value="ServiceType.Procedure">Thủ thuật / Phẫu thuật</option>
          </select>
        </div>
      </div>

      <!-- Service Name -->
      <div>
        <label class="block text-xs font-semibold text-slate-700 mb-1">
          Tên dịch vụ kỹ thuật <span class="text-rose-500">*</span>
        </label>
        <input
          v-model="form.name"
          type="text"
          required
          placeholder="VD: Khám chuyên khoa Nội tổng quát, Tổng phân tích tế bào máu..."
          class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-medium"
        />
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Department -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Khoa / Phòng thực hiện</label>
          <select
            v-model="form.departmentId"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          >
            <option :value="undefined">-- Chọn khoa phòng thực hiện --</option>
            <option v-for="d in departments" :key="d.id" :value="d.id">
              [{{ d.code }}] {{ d.name }}
            </option>
          </select>
        </div>

        <!-- Unit -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Đơn vị tính</label>
          <input
            v-model="form.unitName"
            type="text"
            placeholder="Lần, Tiêu bản, Vị trí..."
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Price -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Giá viện phí (VNĐ) <span class="text-rose-500">*</span>
          </label>
          <input
            v-model.number="form.price"
            type="number"
            min="0"
            step="1000"
            required
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-semibold"
          />
        </div>

        <!-- Insurance Price -->
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Giá BHYT chi trả (VNĐ)
          </label>
          <input
            v-model.number="form.insurancePrice"
            type="number"
            min="0"
            step="1000"
            placeholder="Bỏ trống nếu không áp dụng BHYT"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-emerald-700 font-semibold"
          />
        </div>
      </div>

      <!-- Description -->
      <div>
        <label class="block text-xs font-semibold text-slate-700 mb-1">Ghi chú / Mô tả kỹ thuật</label>
        <textarea
          v-model="form.description"
          rows="2"
          placeholder="Mô tả quy trình, vật tư tiêu hao đi kèm..."
          class="w-full px-3.5 py-2 text-xs bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
        />
      </div>

      <!-- Active switch -->
      <div class="flex items-center gap-2 pt-1">
        <input
          v-model="form.isActive"
          type="checkbox"
          id="isActive"
          class="rounded border-slate-300 text-primary focus:ring-primary w-4 h-4"
        />
        <label for="isActive" class="text-xs font-semibold text-slate-700 cursor-pointer">
          Kích hoạt sử dụng ngay trong hệ thống
        </label>
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
        <PlusCircle v-else class="w-4 h-4" />
        <span>Lưu dịch vụ</span>
      </button>
    </template>
  </BaseModal>
</template>

