<script setup lang="ts">
import { BedDouble, GitBranch, FileText, CheckCircle } from 'lucide-vue-next'
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-violet-500 to-purple-600 flex items-center justify-center text-white shadow-md">
          <BedDouble class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Phân hệ Quản Lý Nội Trú (Inpatient)</h1>
          <p class="text-xs text-slate-500 mt-0.5">Sơ đồ giường trực quan, nhập/chuyển viện, y lệnh điều trị và hồ sơ bệnh án nội trú</p>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
      <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
        <BedDouble class="w-8 h-8 text-violet-600 mb-2" />
        <h3 class="font-bold text-slate-800 text-sm">Sơ đồ giường khoa phòng</h3>
        <p class="text-xs text-slate-500 mt-1">Trạng thái phòng bệnh, xếp giường và theo dõi công suất</p>
      </div>
      <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
        <FileText class="w-8 h-8 text-purple-600 mb-2" />
        <h3 class="font-bold text-slate-800 text-sm">Y lệnh & Theo dõi</h3>
        <p class="text-xs text-slate-500 mt-1">Y lệnh thuốc, chăm sóc điều dưỡng, phiếu theo dõi sinh hiệu</p>
      </div>
      <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
        <CheckCircle class="w-8 h-8 text-emerald-600 mb-2" />
        <h3 class="font-bold text-slate-800 text-sm">Tổng kết & Ra viện</h3>
        <p class="text-xs text-slate-500 mt-1">Quyết toán viện phí nội trú, trích xuất dữ liệu BHYT</p>
      </div>
    </div>
  </div>
</template>

