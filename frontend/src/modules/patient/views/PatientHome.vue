<script setup lang="ts">
import { Users, Fingerprint, History } from 'lucide-vue-next'
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-cyan-500 to-blue-600 flex items-center justify-center text-white shadow-md">
          <Users class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Hồ sơ Bệnh nhân (Master Patient Index - MPI)</h1>
          <p class="text-xs text-slate-500 mt-0.5">Định danh duy nhất CCCD, thẻ BHYT, tiền sử bệnh án và thông tin liên hệ dùng chung</p>
        </div>
      </div>
    </div>
  </div>
</template>

