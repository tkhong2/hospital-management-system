<script setup lang="ts">
import { onMounted } from 'vue'
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
  UserCheck
} from 'lucide-vue-next'

const authStore = useAuthStore()
const launcherStore = useAppLauncherStore()
const router = useRouter()

onMounted(async () => {
  if (authStore.isAuthenticated && authStore.applications.length === 0) {
    await authStore.fetchMyApplications()
  }
})

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
  { label: 'Lượt tiếp đón hôm nay', value: '142', icon: UserCheck, color: 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-950/50' },
  { label: 'Hồ sơ khám sức khỏe đoàn', value: '38', icon: HeartPulse, color: 'text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950/50' },
  { label: 'Tỷ lệ lấp đầy giường bệnh', value: '86%', icon: BedDouble, color: 'text-violet-600 dark:text-violet-400 bg-violet-50 dark:bg-violet-950/50' },
  { label: 'Đơn thuốc chờ cấp phát', value: '15', icon: Pill, color: 'text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/50' },
]

function navigateTo(path: string, appCode: string) {
  if (!authStore.canAccessApp(appCode)) return
  router.push(path)
}
</script>

<template>
  <div class="space-y-8">
    <!-- Welcome Banner -->
    <div class="relative overflow-hidden rounded-2xl bg-gradient-to-r from-hospital-700 via-hospital-800 to-slate-900 p-8 text-white shadow-lg">
      <div class="relative z-10 max-w-2xl">
        <span class="inline-block px-3 py-1 rounded-full bg-hospital-500/30 text-hospital-200 text-xs font-semibold uppercase tracking-wider mb-3">
          Cổng Ứng Dụng Tích Hợp
        </span>
        <h1 class="text-3xl font-extrabold tracking-tight sm:text-4xl">
          Xin chào, {{ authStore.user?.fullName }}!
        </h1>
        <p class="mt-2 text-sm text-hospital-100/90 leading-relaxed">
          Hệ thống Quản lý Bệnh viện Tích hợp - Microservices HIS chuẩn Bộ Y Tế. Quản lý toàn diện từ Khám sức khỏe đoàn, Ngoại trú, Nội trú đến Kho dược thông minh.
        </p>
      </div>
      <div class="absolute -right-10 -bottom-10 w-72 h-72 bg-hospital-500/20 rounded-full blur-3xl pointer-events-none"></div>
    </div>

    <!-- Quick Stats -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div
        v-for="(stat, idx) in stats"
        :key="idx"
        class="bg-white dark:bg-slate-900 rounded-xl p-5 border border-slate-200/80 dark:border-slate-800 shadow-sm flex items-center gap-4 transition-colors"
      >
        <div :class="['w-12 h-12 rounded-xl flex items-center justify-center shrink-0', stat.color]">
          <component :is="stat.icon" class="w-6 h-6" />
        </div>
        <div>
          <div class="text-2xl font-bold text-slate-900 dark:text-white">{{ stat.value }}</div>
          <div class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">{{ stat.label }}</div>
        </div>
      </div>
    </div>

    <!-- Application Launcher Grid -->
    <div>
      <div class="flex items-center justify-between mb-4">
        <div>
          <h2 class="text-lg font-bold text-slate-900 dark:text-white">Phân hệ Chuyên môn Được Cấp Quyền</h2>
          <p class="text-xs text-slate-500 dark:text-slate-400">Các phân hệ được kích hoạt dựa trên vai trò và ma trận quyền hạn người dùng</p>
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
        <div
          v-for="app in launcherStore.apps"
          :key="app.id"
          @click="navigateTo(app.path, app.code)"
          class="bg-white dark:bg-slate-900 rounded-xl border border-slate-200/90 dark:border-slate-800 p-5 shadow-sm transition-all flex flex-col justify-between"
          :class="[
            authStore.canAccessApp(app.code)
              ? 'hover:border-hospital-400 dark:hover:border-hospital-500 hover:shadow-md cursor-pointer group'
              : 'opacity-40 grayscale cursor-not-allowed select-none'
          ]"
        >
          <div>
            <div class="flex items-center justify-between mb-3">
              <div
                :class="[
                  'w-12 h-12 rounded-xl bg-gradient-to-br flex items-center justify-center text-white shadow-sm transition-transform',
                  authStore.canAccessApp(app.code) ? 'group-hover:scale-105' : '',
                  app.color
                ]"
              >
                <component :is="iconMap[app.icon]" class="w-6 h-6" />
              </div>
              <div class="flex items-center gap-1.5">
                <span
                  v-if="app.badge"
                  class="px-2.5 py-1 text-[11px] font-semibold bg-emerald-100 dark:bg-emerald-950 text-emerald-800 dark:text-emerald-300 rounded-full"
                >
                  {{ app.badge }}
                </span>
                <span
                  v-if="!authStore.canAccessApp(app.code)"
                  class="px-2 py-0.5 text-[10px] font-semibold bg-slate-100 dark:bg-slate-800 text-slate-500 rounded"
                >
                  Chưa cấp quyền
                </span>
              </div>
            </div>

            <h3
              class="font-bold text-slate-900 dark:text-white text-base transition-colors"
              :class="authStore.canAccessApp(app.code) ? 'group-hover:text-hospital-600 dark:group-hover:text-hospital-400' : ''"
            >
              {{ app.name }}
            </h3>
            <p class="text-xs text-slate-500 dark:text-slate-400 mt-1.5 leading-relaxed line-clamp-2">
              {{ app.description }}
            </p>
          </div>

          <div
            v-if="authStore.canAccessApp(app.code)"
            class="mt-4 pt-3 border-t border-slate-100 dark:border-slate-800 flex items-center justify-between text-xs text-hospital-600 dark:text-hospital-400 font-medium group-hover:translate-x-0.5 transition-transform"
          >
            <span>Mở phân hệ</span>
            <span>&rarr;</span>
          </div>
          <div
            v-else
            class="mt-4 pt-3 border-t border-slate-100 dark:border-slate-800 text-xs text-slate-400 font-medium"
          >
            <span>Liên hệ Quản trị viên để xin cấp quyền</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
