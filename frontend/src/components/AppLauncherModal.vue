<script setup lang="ts">
import { useAppLauncherStore } from '@/stores/appLauncher'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'
import { 
  X, 
  HeartPulse, 
  Stethoscope, 
  BedDouble, 
  Pill, 
  Users, 
  Database, 
  ShieldCheck 
} from 'lucide-vue-next'

const launcherStore = useAppLauncherStore()
const authStore = useAuthStore()
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

function openApp(path: string) {
  launcherStore.closeLauncher()
  router.push(path)
}
</script>

<template>
  <div
    v-if="launcherStore.isLauncherOpen"
    class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-sm"
    @click.self="launcherStore.closeLauncher"
  >
    <div class="bg-white rounded-2xl shadow-2xl max-w-4xl w-full p-6 border border-slate-200 animate-in fade-in zoom-in-95 duration-150">
      <div class="flex items-center justify-between pb-4 border-b border-slate-100 mb-6">
        <div>
          <h2 class="text-xl font-bold text-slate-900">Danh mục Ứng dụng Bệnh viện</h2>
          <p class="text-xs text-slate-500 mt-1">Chọn phân hệ làm việc theo quyền hạn tài khoản của bạn</p>
        </div>
        <button
          @click="launcherStore.closeLauncher"
          class="p-2 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg transition-colors"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <button
          v-for="app in launcherStore.apps"
          :key="app.id"
          @click="openApp(app.path)"
          class="flex items-start gap-4 p-4 rounded-xl border border-slate-200/80 hover:border-hospital-400 hover:shadow-md transition-all text-left group bg-white hover:bg-hospital-50/30 relative"
          :class="{ 'opacity-50 pointer-events-none': !authStore.canAccessApp(app.code) }"
        >
          <div
            :class="[
              'w-12 h-12 rounded-xl bg-gradient-to-br flex items-center justify-center text-white shadow-sm shrink-0 group-hover:scale-105 transition-transform',
              app.color
            ]"
          >
            <component :is="iconMap[app.icon]" class="w-6 h-6" />
          </div>

          <div class="min-w-0 flex-1">
            <div class="flex items-center gap-2 mb-1">
              <span class="font-semibold text-slate-800 text-sm group-hover:text-hospital-600 transition-colors">
                {{ app.name }}
              </span>
              <span
                v-if="app.badge"
                class="px-2 py-0.5 text-[10px] font-semibold bg-emerald-100 text-emerald-700 rounded-full"
              >
                {{ app.badge }}
              </span>
            </div>
            <p class="text-xs text-slate-500 line-clamp-2 leading-relaxed">
              {{ app.description }}
            </p>
          </div>
        </button>
      </div>
    </div>
  </div>
</template>

