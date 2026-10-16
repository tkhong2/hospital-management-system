<script setup lang="ts">
import { Database, FileText, Building, Award } from 'lucide-vue-next'
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-slate-600 to-slate-800 flex items-center justify-center text-white shadow-md">
          <Database class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Danh Mục Dùng Chung (Catalog)</h1>
          <p class="text-xs text-slate-500 mt-0.5">ICD-10, dịch vụ kỹ thuật Thông tư 39, danh mục thuốc, khoa phòng, chuyên khoa</p>
        </div>
      </div>
    </div>
  </div>
</template>

