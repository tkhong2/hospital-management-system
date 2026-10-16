<script setup lang="ts">
import { ShieldCheck, UserCheck, Key, ShieldAlert } from 'lucide-vue-next'
</script>

<template>
  <div class="space-y-6">
    <div class="flex items-center justify-between bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-rose-500 to-red-600 flex items-center justify-center text-white shadow-md">
          <ShieldCheck class="w-6 h-6" />
        </div>
        <div>
          <h1 class="text-xl font-bold text-slate-900">Quản Trị Hệ Thống & Định Danh (Identity)</h1>
          <p class="text-xs text-slate-500 mt-0.5">Quản lý người dùng, vai trò (Roles), ma trận quyền (Permissions) và App Launcher</p>
        </div>
      </div>
    </div>
  </div>
</template>

