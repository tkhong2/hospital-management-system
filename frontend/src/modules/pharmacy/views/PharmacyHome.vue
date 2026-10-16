<script setup lang="ts">
import { Pill, Layers, History, ShieldAlert } from 'lucide-vue-next'
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center text-white shadow-md">
          <Pill class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Phân hệ Kho Dược & Vật Tư (Pharmacy)</h1>
          <p class="text-xs text-slate-500 mt-0.5">Quản lý kho chẵn/kho lẻ/tủ trực, theo dõi lô hạn dùng theo nguyên tắc FEFO/FIFO</p>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
      <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
        <Layers class="w-8 h-8 text-amber-600 mb-2" />
        <h3 class="font-bold text-slate-800 text-sm">Nhập - Xuất - Tồn</h3>
        <p class="text-xs text-slate-500 mt-1">Phiếu nhập kho nhà cung cấp, xuất chuyển kho nội bộ</p>
      </div>
      <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
        <History class="w-8 h-8 text-orange-600 mb-2" />
        <h3 class="font-bold text-slate-800 text-sm">Quản lý Lô - Hạn dùng (FEFO)</h3>
        <p class="text-xs text-slate-500 mt-1">Cảnh báo thuốc cận hạn, kiểm soát số lô chính xác</p>
      </div>
      <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
        <ShieldAlert class="w-8 h-8 text-rose-600 mb-2" />
        <h3 class="font-bold text-slate-800 text-sm">Duyệt & Cấp phát đơn thuốc</h3>
        <p class="text-xs text-slate-500 mt-1">Duyệt đơn ngoại trú, cấp phát theo y lệnh nội trú</p>
      </div>
    </div>
  </div>
</template>

