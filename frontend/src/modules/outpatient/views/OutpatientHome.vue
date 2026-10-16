<script setup lang="ts">
import { Stethoscope, UserPlus, ClipboardList, Receipt } from 'lucide-vue-next'
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center text-white shadow-md">
          <Stethoscope class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Phân hệ Khám Ngoại Trú (Outpatient)</h1>
          <p class="text-xs text-slate-500 mt-0.5">Tiếp đón bệnh nhân, phòng khám chuyên khoa, chỉ định CLS và kê đơn điện tử</p>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
      <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
        <UserPlus class="w-8 h-8 text-blue-600 mb-2" />
        <h3 class="font-bold text-slate-800 text-sm">Tiếp đón & Cấp số</h3>
        <p class="text-xs text-slate-500 mt-1">Check-in CCCD/BHYT, phân luồng phòng khám tự động</p>
      </div>
      <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
        <ClipboardList class="w-8 h-8 text-indigo-600 mb-2" />
        <h3 class="font-bold text-slate-800 text-sm">Buồng khám bác sĩ</h3>
        <p class="text-xs text-slate-500 mt-1">Chẩn đoán ICD-10, chỉ định xét nghiệm/CĐHA và kê đơn</p>
      </div>
      <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
        <Receipt class="w-8 h-8 text-teal-600 mb-2" />
        <h3 class="font-bold text-slate-800 text-sm">Thu viện phí & BHYT</h3>
        <p class="text-xs text-slate-500 mt-1">Tính toán đồng chi trả BHYT, xuất hóa đơn điện tử</p>
      </div>
    </div>
  </div>
</template>

