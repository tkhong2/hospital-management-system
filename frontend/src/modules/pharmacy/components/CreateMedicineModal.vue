<script setup lang="ts">
import { ref, reactive } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { pharmacyApi } from '../api/pharmacyApi'
import { MedicineType, SpecialControlType } from '../types/pharmacy'
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

const form = reactive({
  code: '',
  name: '',
  registrationNumber: '',
  activeIngredient: '',
  concentration: '',
  unitName: 'Viên',
  packageSpecification: '',
  routeOfAdministration: 'Đường uống',
  medicineGroup: '',
  medicineType: MedicineType.Medicine,
  specialControl: SpecialControlType.None,
  inboundPrice: 1000,
  salePrice: 1300,
  insurancePrice: 1100,
  minStockLevel: 100,
  maxStockLevel: 5000
})

const resetForm = () => {
  form.code = ''
  form.name = ''
  form.registrationNumber = ''
  form.activeIngredient = ''
  form.concentration = ''
  form.unitName = 'Viên'
  form.packageSpecification = ''
  form.routeOfAdministration = 'Đường uống'
  form.medicineGroup = ''
  form.medicineType = MedicineType.Medicine
  form.specialControl = SpecialControlType.None
  form.inboundPrice = 1000
  form.salePrice = 1300
  form.insurancePrice = 1100
  form.minStockLevel = 100
  form.maxStockLevel = 5000
  errorMessage.value = ''
}

const handleSubmit = async () => {
  if (!form.code.trim()) {
    errorMessage.value = 'Mã thuốc không được để trống'
    return
  }
  if (!form.name.trim()) {
    errorMessage.value = 'Tên thuốc không được để trống'
    return
  }
  if (!form.activeIngredient.trim()) {
    errorMessage.value = 'Hoạt chất không được để trống'
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const res = await pharmacyApi.createMedicine({
      code: form.code.trim().toUpperCase(),
      name: form.name.trim(),
      registrationNumber: form.registrationNumber?.trim() || undefined,
      activeIngredient: form.activeIngredient.trim(),
      concentration: form.concentration.trim(),
      unitName: form.unitName.trim(),
      packageSpecification: form.packageSpecification?.trim() || undefined,
      routeOfAdministration: form.routeOfAdministration?.trim() || undefined,
      medicineGroup: form.medicineGroup?.trim() || undefined,
      medicineType: Number(form.medicineType),
      specialControl: Number(form.specialControl),
      inboundPrice: Number(form.inboundPrice),
      salePrice: Number(form.salePrice),
      insurancePrice: form.insurancePrice ? Number(form.insurancePrice) : undefined,
      minStockLevel: Number(form.minStockLevel),
      maxStockLevel: Number(form.maxStockLevel)
    })

    if (res.success) {
      resetForm()
      emit('saved')
      emit('update:modelValue', false)
    } else {
      errorMessage.value = res.errors?.[0] || 'Lỗi khi thêm mới thuốc'
    }
  } catch (err: any) {
    errorMessage.value = err?.message || 'Có lỗi xảy ra khi lưu thuốc'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <BaseModal
    :model-value="modelValue"
    @update:model-value="emit('update:modelValue', $event)"
    title="Thêm Mới Thuốc / Vật Tư Y Tế"
    subtitle="Khai báo danh mục thuốc, hoạt chất, hàm lượng, giá viện phí và định mức tồn kho"
    max-width="3xl"
  >
    <div
      v-if="errorMessage"
      class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs font-medium"
    >
      {{ errorMessage }}
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Mã thuốc <span class="text-rose-500">*</span>
          </label>
          <input
            v-model="form.code"
            type="text"
            required
            placeholder="VD: PARA500"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary uppercase font-mono font-medium text-slate-800"
          />
        </div>

        <div class="md:col-span-2">
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Tên thuốc / Biệt dược <span class="text-rose-500">*</span>
          </label>
          <input
            v-model="form.name"
            type="text"
            required
            placeholder="VD: Panadol Extra 500mg"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary font-medium text-slate-800"
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Hoạt chất <span class="text-rose-500">*</span>
          </label>
          <input
            v-model="form.activeIngredient"
            type="text"
            required
            placeholder="Paracetamol"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Hàm lượng <span class="text-rose-500">*</span>
          </label>
          <input
            v-model="form.concentration"
            type="text"
            required
            placeholder="500mg"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Đơn vị tính</label>
          <input
            v-model="form.unitName"
            type="text"
            placeholder="Viên, Ống, Lọ, Gói..."
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Phân loại danh mục</label>
          <select
            v-model="form.medicineType"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          >
            <option :value="MedicineType.Medicine">Thuốc</option>
            <option :value="MedicineType.MedicalSupply">Vật tư y tế tiêu hao</option>
            <option :value="MedicineType.Chemical">Hóa chất xét nghiệm</option>
          </select>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Kiểm soát đặc biệt</label>
          <select
            v-model="form.specialControl"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-medium"
          >
            <option :value="SpecialControlType.None">Thuốc thông thường</option>
            <option :value="SpecialControlType.Narcotic">Thuốc gây nghiện</option>
            <option :value="SpecialControlType.Psychotropic">Thuốc hướng thần</option>
            <option :value="SpecialControlType.Precursor">Thuốc tiền chất</option>
            <option :value="SpecialControlType.Toxic">Thuốc độc</option>
            <option :value="SpecialControlType.Antibiotic">Kháng sinh quản lý</option>
          </select>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Số đăng ký lưu hành (SĐK)</label>
          <input
            v-model="form.registrationNumber"
            type="text"
            placeholder="VD: VN-18234-14"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-mono"
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Giá nhập dự kiến (VNĐ)</label>
          <input
            v-model.number="form.inboundPrice"
            type="number"
            min="0"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-semibold"
          />
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Giá bán viện phí (VNĐ)</label>
          <input
            v-model.number="form.salePrice"
            type="number"
            min="0"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-semibold"
          />
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Giá BHYT chi trả (VNĐ)</label>
          <input
            v-model.number="form.insurancePrice"
            type="number"
            min="0"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-emerald-700 font-semibold"
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Tồn tối thiểu (Cảnh báo đặt hàng)</label>
          <input
            v-model.number="form.minStockLevel"
            type="number"
            min="0"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-amber-700 font-semibold"
          />
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1">Tồn tối đa</label>
          <input
            v-model.number="form.maxStockLevel"
            type="number"
            min="0"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-semibold"
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
        <PlusCircle v-else class="w-4 h-4" />
        <span>Lưu thuốc vào danh mục</span>
      </button>
    </template>
  </BaseModal>
</template>

