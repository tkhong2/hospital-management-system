<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useAppLauncherStore } from '@/stores/appLauncher'
import { useRouter } from 'vue-router'
import {
  HeartPulse,
  Stethoscope,
  BedDouble,
  Pill,
  Users,
  Database,
  ShieldCheck,
  Activity,
  UserCheck,
  FileCheck,
  Calendar
} from 'lucide-vue-next'

const authStore = useAuthStore()
const launcherStore = useAppLauncherStore()
const router = useRouter()

const iconMap: Record<string, any> = {
  HeartPulse,
  Stethoscope,
  BedDouble,
  Pill,
  Users,
  Database,
  ShieldCheck
}

const stats = [
  { label: 'Lượt tiếp đón hôm nay', value: '142', icon: UserCheck, color: 'text-blue-600 bg-blue-50' },
  { label: 'Hồ sơ khám sức khỏe đang xử lý', value: '38', icon: HeartPulse, color: 'text-emerald-600 bg-emerald-50' },
  { label: 'Giường nội trú đang sử dụng', value: '86%', icon: BedDouble, color: 'text-violet-600 bg-violet-50' },
  { label: 'Đơn thuốc chờ cấp phát', value: '15', icon: Pill, color: 'text-amber-600 bg-amber-50' },
]

function navigateTo(path: string) {
  router.push(path)
}
</script>

<template>
  <div class="space-y-8">
    <!-- Welcome Banner -->
    <div class="relative overflow-hidden rounded-2xl bg-gradient-to-r from-hospital-700 via-hospital-800 to-slate-900 p-8 text-white shadow-lg">
      <div class="relative z-10 max-w-2xl">
        <span class="inline-block px-3 py-1 rounded-full bg-hospital-500/30 text-hospital-200 text-xs font-semibold uppercase tracking-wider mb-3">
          Cổng Ứng Dụng Tổng Hợp
        </span>
        <h1 class="text-3xl font-extrabold tracking-tight sm:text-4xl">
          Xin chào, {{ authStore.user?.fullName }}!
        </h1>
        <p class="mt-2 text-sm text-hospital-100/90 leading-relaxed">
          Hệ thống Quản lý Bệnh viện Tích hợp hỗ trợ kết nối đa phân hệ từ Khám sức khỏe đoàn, Ngoại trú, Nội trú đến Quản lý Kho dược thông minh.
        </p>
      </div>
      <div class="absolute -right-10 -bottom-10 w-72 h-72 bg-hospital-500/20 rounded-full blur-3xl pointer-events-none"></div>
    </div>

    <!-- Quick Stats -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div
        v-for="(stat, idx) in stats"
        :key="idx"
        class="bg-white rounded-xl p-5 border border-slate-200/80 shadow-sm flex items-center gap-4"
      >
        <div :class="['w-12 h-12 rounded-xl flex items-center justify-center shrink-0', stat.color]">
          <component :is="stat.icon" class="w-6 h-6" />
        </div>
        <div>
          <div class="text-2xl font-bold text-slate-900">{{ stat.value }}</div>
          <div class="text-xs text-slate-500 mt-0.5">{{ stat.label }}</div>
        </div>
      </div>
    </div>

    <!-- Application Launcher Grid -->
    <div>
      <div class="flex items-center justify-between mb-4">
        <div>
          <h2 class="text-lg font-bold text-slate-900">Phân hệ Chuyên môn</h2>
          <p class="text-xs text-slate-500">Truy cập nhanh vào các ứng dụng nghiệp vụ được cấp quyền</p>
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        <div
          v-for="app in launcherStore.apps"
          :key="app.id"
          @click="navigateTo(app.path)"
          class="bg-white rounded-xl border border-slate-200/90 hover:border-hospital-400 p-5 shadow-sm hover:shadow-md transition-all cursor-pointer group flex flex-col justify-between"
        >
          <div>
            <div class="flex items-center justify-between mb-3">
              <div
                :class="[
                  'w-12 h-12 rounded-xl bg-gradient-to-br flex items-center justify-center text-white shadow-sm group-hover:scale-105 transition-transform',
                  app.color
                ]"
              >
                <component :is="iconMap[app.icon]" class="w-6 h-6" />
              </div>
              <span
                v-if="app.badge"
                class="px-2.5 py-1 text-[11px] font-semibold bg-emerald-100 text-emerald-800 rounded-full"
              >
                {{ app.badge }}
              </span>
            </div>

            <h3 class="font-bold text-slate-900 text-base group-hover:text-hospital-600 transition-colors">
              {{ app.name }}
            </h3>
            <p class="text-xs text-slate-500 mt-1.5 leading-relaxed line-clamp-2">
              {{ app.description }}
            </p>
          </div>

          <div class="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between text-xs text-hospital-600 font-medium group-hover:translate-x-0.5 transition-transform">
            <span>Mở phân hệ</span>
            <span>&rarr;</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

