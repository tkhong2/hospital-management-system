<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import BaseModal from '@/core/components/BaseModal.vue'
import { pharmacyApi } from '../api/pharmacyApi'
import { DocumentType, type Warehouse, type Medicine } from '../types/pharmacy'
import { Plus, Trash2, FileText, Loader2 } from 'lucide-vue-next'

const props = defineProps<{
  modelValue: boolean
  warehouses: Warehouse[]
  medicines: Medicine[]
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'saved'): void
}>()

const isSubmitting = ref(false)
const errorMessage = ref('')

const form = reactive({
  documentType: DocumentType.Inbound,
  sourceWarehouseId: '',
  targetWarehouseId: '',
  supplierName: 'Công ty Cổ phần Dược phẩm Trung ương 1',
  note: '',
  items: [
    {
      medicineId: '',
      batchNumber: 'L260301',
      expiryDate: new Date(Date.now() + 365 * 24 * 3600 * 1000).toISOString().slice(0, 10),
      quantity: 100,
      unitPrice: 1200,
      note: ''
    }
  ]
})

const addItem = () => {
  form.items.push({
    medicineId: '',
    batchNumber: `L${new Date().toISOString().slice(2, 7).replace('-', '')}`,
    expiryDate: new Date(Date.now() + 365 * 24 * 3600 * 1000).toISOString().slice(0, 10),
    quantity: 100,
    unitPrice: 1000,
    note: ''
  })
}

const removeItem = (idx: number) => {
  if (form.items.length > 1) {
    form.items.splice(idx, 1)
  }
}

const onMedicineSelected = (idx: number, medicineId: string) => {
  const med = props.medicines.find((m) => m.id === medicineId)
  if (med) {
    form.items[idx].unitPrice = form.documentType === DocumentType.Inbound ? med.inboundPrice : med.salePrice
  }
}

const totalDocumentAmount = computed(() => {
  return form.items.reduce((sum, item) => sum + (item.quantity * item.unitPrice || 0), 0)
})

const handleSubmit = async () => {
  if (form.documentType === DocumentType.Inbound && !form.targetWarehouseId) {
    errorMessage.value = 'Vui lòng chọn Kho nhập hàng'
    return
  }
  if (form.documentType === DocumentType.Transfer && (!form.sourceWarehouseId || !form.targetWarehouseId)) {
    errorMessage.value = 'Chuyển kho yêu cầu chọn cả Kho xuất và Kho nhận'
    return
  }
  if (form.items.some((i) => !i.medicineId || i.quantity <= 0)) {
    errorMessage.value = 'Vui lòng chọn thuốc và nhập số lượng hợp lệ cho tất cả dòng'
    return
  }

  isSubmitting.value = true
  errorMessage.value = ''

  try {
    const res = await pharmacyApi.createStockDocument({
      documentType: form.documentType,
      sourceWarehouseId: form.sourceWarehouseId || undefined,
      targetWarehouseId: form.targetWarehouseId || undefined,
      supplierName: form.supplierName?.trim() || undefined,
      note: form.note?.trim() || undefined,
      details: form.items.map((i) => ({
        medicineId: i.medicineId,
        batchNumber: i.batchNumber.trim().toUpperCase(),
        expiryDate: new Date(i.expiryDate).toISOString(),
        quantity: Number(i.quantity),
        unitPrice: Number(i.unitPrice),
        totalPrice: Number(i.quantity) * Number(i.unitPrice),
        note: i.note?.trim() || undefined
      }))
    })

    if (res.success) {
      emit('saved')
      emit('update:modelValue', false)
    } else {
      errorMessage.value = res.errors?.[0] || 'Lỗi khi tạo phiếu kho'
    }
  } catch (err: any) {
    errorMessage.value = err?.message || 'Có lỗi xảy ra khi lưu phiếu kho'
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
    title="Lập Phiếu Kho Dược"
    subtitle="Lập phiếu nhập kho từ nhà cung cấp, xuất chuyển kho hoặc xuất hủy thuốc hỏng/hết hạn"
    max-width="4xl"
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
          <label class="block text-xs font-semibold text-slate-700 mb-1">Loại phiếu kho</label>
          <select
            v-model="form.documentType"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800 font-semibold"
          >
            <option :value="DocumentType.Inbound">Phiếu nhập kho (Nhà cung cấp)</option>
            <option :value="DocumentType.Transfer">Phiếu chuyển kho nội bộ</option>
            <option :value="DocumentType.Outbound">Phiếu xuất kho</option>
            <option :value="DocumentType.Dispose">Phiếu xuất hủy thuốc</option>
          </select>
        </div>

        <div v-if="form.documentType === DocumentType.Transfer || form.documentType === DocumentType.Outbound || form.documentType === DocumentType.Dispose">
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Kho xuất hàng <span class="text-rose-500">*</span>
          </label>
          <select
            v-model="form.sourceWarehouseId"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          >
            <option value="">-- Chọn kho xuất --</option>
            <option v-for="w in warehouses" :key="w.id" :value="w.id">
              [{{ w.code }}] {{ w.name }}
            </option>
          </select>
        </div>

        <div v-if="form.documentType === DocumentType.Inbound || form.documentType === DocumentType.Transfer">
          <label class="block text-xs font-semibold text-slate-700 mb-1">
            Kho nhận hàng <span class="text-rose-500">*</span>
          </label>
          <select
            v-model="form.targetWarehouseId"
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          >
            <option value="">-- Chọn kho nhận --</option>
            <option v-for="w in warehouses" :key="w.id" :value="w.id">
              [{{ w.code }}] {{ w.name }}
            </option>
          </select>
        </div>

        <div v-if="form.documentType === DocumentType.Inbound">
          <label class="block text-xs font-semibold text-slate-700 mb-1">Nhà cung cấp / Đơn vị giao</label>
          <input
            v-model="form.supplierName"
            type="text"
            placeholder="CTY Cổ phần Dược phẩm..."
            class="w-full px-3.5 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
          />
        </div>
      </div>

      <!-- Items Table -->
      <div class="border border-slate-200 rounded-xl overflow-hidden mt-2">
        <div class="bg-slate-50 px-4 py-2.5 flex items-center justify-between border-b border-slate-200">
          <span class="text-xs font-bold text-slate-700 uppercase tracking-wider">Danh mục thuốc & số lô xuất / nhập</span>
          <button
            type="button"
            @click="addItem"
            class="inline-flex items-center gap-1 text-xs text-primary font-bold hover:text-primary-hover"
          >
            <Plus class="w-3.5 h-3.5" />
            <span>Thêm dòng</span>
          </button>
        </div>

        <div class="p-3 space-y-3">
          <div
            v-for="(item, idx) in form.items"
            :key="idx"
            class="grid grid-cols-12 gap-2 items-center bg-white p-2 rounded-lg border border-slate-100 shadow-xs"
          >
            <div class="col-span-4">
              <label class="block text-[10px] font-semibold text-slate-500 mb-0.5">Thuốc / Vật tư</label>
              <select
                v-model="item.medicineId"
                @change="onMedicineSelected(idx, item.medicineId)"
                class="w-full px-2 py-1.5 text-xs bg-white border border-slate-200 rounded-lg text-slate-800 font-medium"
              >
                <option value="" disabled>-- Chọn thuốc --</option>
                <option v-for="m in medicines" :key="m.id" :value="m.id">
                  [{{ m.code }}] {{ m.name }} ({{ m.unitName }})
                </option>
              </select>
            </div>

            <div class="col-span-2">
              <label class="block text-[10px] font-semibold text-slate-500 mb-0.5">Số lô</label>
              <input
                v-model="item.batchNumber"
                type="text"
                class="w-full px-2 py-1.5 text-xs bg-white border border-slate-200 rounded-lg font-mono uppercase text-slate-800"
              />
            </div>

            <div class="col-span-2">
              <label class="block text-[10px] font-semibold text-slate-500 mb-0.5">Hạn dùng</label>
              <input
                v-model="item.expiryDate"
                type="date"
                class="w-full px-2 py-1.5 text-xs bg-white border border-slate-200 rounded-lg text-slate-800"
              />
            </div>

            <div class="col-span-1">
              <label class="block text-[10px] font-semibold text-slate-500 mb-0.5">Số lượng</label>
              <input
                v-model.number="item.quantity"
                type="number"
                min="1"
                class="w-full px-2 py-1.5 text-xs bg-white border border-slate-200 rounded-lg text-slate-800 font-semibold text-right"
              />
            </div>

            <div class="col-span-2">
              <label class="block text-[10px] font-semibold text-slate-500 mb-0.5">Đơn giá</label>
              <input
                v-model.number="item.unitPrice"
                type="number"
                min="0"
                class="w-full px-2 py-1.5 text-xs bg-white border border-slate-200 rounded-lg text-slate-800 text-right"
              />
            </div>

            <div class="col-span-1 text-center pt-3">
              <button
                type="button"
                @click="removeItem(idx)"
                class="text-slate-400 hover:text-rose-600 p-1"
                :disabled="form.items.length <= 1"
              >
                <Trash2 class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        <div class="bg-slate-50 px-4 py-2 border-t border-slate-200 flex items-center justify-between text-xs">
          <span class="text-slate-600 font-medium">Tổng giá trị phiếu:</span>
          <span class="text-sm font-bold text-primary">{{ formatCurrency(totalDocumentAmount) }}</span>
        </div>
      </div>

      <div>
        <label class="block text-xs font-semibold text-slate-700 mb-1">Ghi chú phiếu kho</label>
        <textarea
          v-model="form.note"
          rows="2"
          placeholder="Lý do xuất nhập, hóa đơn kèm theo..."
          class="w-full px-3.5 py-2 text-xs bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-primary/20 focus:border-primary text-slate-800"
        />
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
        <FileText v-else class="w-4 h-4" />
        <span>Lưu phiếu kho (Nháp)</span>
      </button>
    </template>
  </BaseModal>
</template>

