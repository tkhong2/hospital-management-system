<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { useAppLauncherStore } from '@/stores/appLauncher'
import { useRouter } from 'vue-router'
import { LayoutGrid, Bell, User, LogOut } from 'lucide-vue-next'

const authStore = useAuthStore()
const launcherStore = useAppLauncherStore()
const router = useRouter()

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <header class="h-16 bg-white border-b border-slate-200 px-6 flex items-center justify-between sticky top-0 z-20">
    <div class="flex items-center gap-4">
      <button
        @click="launcherStore.toggleLauncher"
        class="p-2 rounded-lg text-slate-600 hover:text-slate-900 hover:bg-slate-100 transition-colors flex items-center gap-2 font-medium text-sm"
        title="Danh sách ứng dụng"
      >
        <LayoutGrid class="w-5 h-5 text-hospital-600" />
        <span class="hidden sm:inline">Ứng dụng</span>
      </button>

      <div class="h-6 w-px bg-slate-200"></div>

      <router-link to="/" class="flex items-center gap-2">
        <div class="w-8 h-8 rounded-lg bg-gradient-to-br from-hospital-500 to-hospital-700 flex items-center justify-center text-white font-bold text-base shadow-sm">
          +
        </div>
        <div class="leading-tight">
          <div class="font-bold text-slate-800 text-sm tracking-wide">HOSPITAL<span class="text-hospital-600">MS</span></div>
          <div class="text-[10px] text-slate-400 font-medium">Bệnh viện Đa khoa Quốc tế</div>
        </div>
      </router-link>
    </div>

    <div class="flex items-center gap-3">
      <button class="p-2 rounded-lg text-slate-500 hover:text-slate-800 hover:bg-slate-100 relative">
        <Bell class="w-5 h-5" />
        <span class="absolute top-1.5 right-1.5 w-2 h-2 bg-rose-500 rounded-full"></span>
      </button>

      <div class="h-6 w-px bg-slate-200"></div>

      <div class="flex items-center gap-3 pl-2">
        <div class="w-8 h-8 rounded-full bg-hospital-100 text-hospital-700 flex items-center justify-center font-semibold text-xs border border-hospital-200">
          <User class="w-4 h-4" />
        </div>
        <div class="hidden md:block text-left">
          <div class="text-xs font-semibold text-slate-800">{{ authStore.user?.fullName || 'Người dùng' }}</div>
          <div class="text-[10px] text-slate-400">{{ authStore.user?.departmentName || 'Khoa/Phòng' }}</div>
        </div>
        <button
          @click="handleLogout"
          class="p-2 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors ml-1"
          title="Đăng xuất"
        >
          <LogOut class="w-4 h-4" />
        </button>
      </div>
    </div>
  </header>
</template>

