<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useAppLauncherStore } from '@/stores/appLauncher'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { LayoutGrid, Bell, User, LogOut, Sun, Moon, Globe } from 'lucide-vue-next'

const authStore = useAuthStore()
const launcherStore = useAppLauncherStore()
const router = useRouter()
const { locale } = useI18n()

const isDark = ref(false)

onMounted(() => {
  const savedTheme = localStorage.getItem('hospitalms_theme')
  if (savedTheme === 'dark' || (!savedTheme && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    isDark.value = true
    document.documentElement.classList.add('dark')
  } else {
    isDark.value = false
    document.documentElement.classList.remove('dark')
  }

  const savedLocale = localStorage.getItem('hospitalms_locale')
  if (savedLocale && (savedLocale === 'vi' || savedLocale === 'en')) {
    locale.value = savedLocale
  }
})

function toggleTheme() {
  isDark.value = !isDark.value
  if (isDark.value) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('hospitalms_theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('hospitalms_theme', 'light')
  }
}

function toggleLocale() {
  locale.value = locale.value === 'vi' ? 'en' : 'vi'
  localStorage.setItem('hospitalms_locale', locale.value)
}

async function handleLogout() {
  await authStore.logout()
  router.push('/login')
}
</script>

<template>
  <header class="h-16 bg-white dark:bg-slate-900 border-b border-slate-200 dark:border-slate-800 px-6 flex items-center justify-between sticky top-0 z-20 transition-colors">
    <div class="flex items-center gap-4">
      <button
        @click="launcherStore.toggleLauncher"
        class="p-2 rounded-lg text-slate-600 dark:text-slate-300 hover:text-slate-900 dark:hover:text-white hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors flex items-center gap-2 font-medium text-sm"
        title="Danh sách ứng dụng (App Launcher)"
      >
        <LayoutGrid class="w-5 h-5 text-hospital-600 dark:text-hospital-400" />
        <span class="hidden sm:inline">Phân hệ</span>
      </button>

      <div class="h-6 w-px bg-slate-200 dark:bg-slate-800"></div>

      <router-link to="/" class="flex items-center gap-2">
        <div class="w-8 h-8 rounded-lg bg-gradient-to-br from-hospital-500 to-hospital-700 flex items-center justify-center text-white font-bold text-base shadow-sm">
          +
        </div>
        <div class="leading-tight">
          <div class="font-bold text-slate-800 dark:text-white text-sm tracking-wide">
            HOSPITAL<span class="text-hospital-600 dark:text-hospital-400">MS</span>
          </div>
          <div class="text-[10px] text-slate-400 dark:text-slate-500 font-medium">Bệnh viện Đa khoa Quốc tế</div>
        </div>
      </router-link>
    </div>

    <div class="flex items-center gap-2 sm:gap-3">
      <!-- Dark / Light Mode Switch -->
      <button
        @click="toggleTheme"
        class="p-2 rounded-lg text-slate-500 dark:text-slate-400 hover:text-slate-800 dark:hover:text-white hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors"
        :title="isDark ? 'Chuyển sang chế độ sáng' : 'Chuyển sang chế độ tối'"
      >
        <Sun v-if="isDark" class="w-5 h-5 text-amber-400" />
        <Moon v-else class="w-5 h-5" />
      </button>

      <!-- Language Switch (vi / en) -->
      <button
        @click="toggleLocale"
        class="px-2.5 py-1.5 rounded-lg text-xs font-semibold text-slate-600 dark:text-slate-300 hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors flex items-center gap-1.5 border border-slate-200 dark:border-slate-700"
        title="Chuyển đổi ngôn ngữ"
      >
        <Globe class="w-3.5 h-3.5 text-hospital-600 dark:text-hospital-400" />
        <span>{{ locale.toUpperCase() }}</span>
      </button>

      <!-- Notifications -->
      <button class="p-2 rounded-lg text-slate-500 dark:text-slate-400 hover:text-slate-800 dark:hover:text-white hover:bg-slate-100 dark:hover:bg-slate-800 relative transition-colors">
        <Bell class="w-5 h-5" />
        <span class="absolute top-1.5 right-1.5 w-2 h-2 bg-rose-500 rounded-full"></span>
      </button>

      <div class="h-6 w-px bg-slate-200 dark:bg-slate-800"></div>

      <!-- User Menu -->
      <div class="flex items-center gap-2.5 pl-1 sm:pl-2">
        <div class="w-8 h-8 rounded-full bg-hospital-100 dark:bg-hospital-950 text-hospital-700 dark:text-hospital-300 flex items-center justify-center font-semibold text-xs border border-hospital-200 dark:border-hospital-800 shrink-0">
          <User class="w-4 h-4" />
        </div>
        <div class="hidden md:block text-left">
          <div class="text-xs font-semibold text-slate-800 dark:text-slate-100">{{ authStore.user?.fullName || 'Người dùng' }}</div>
          <div class="text-[10px] text-slate-400 dark:text-slate-500 flex items-center gap-1">
            <span>{{ authStore.user?.departmentName || 'Khoa/Phòng' }}</span>
            <span v-if="authStore.user?.roles?.length" class="inline-block px-1.5 py-0.2 bg-slate-100 dark:bg-slate-800 text-[9px] rounded font-medium text-slate-600 dark:text-slate-300">
              {{ authStore.user.roles[0] }}
            </span>
          </div>
        </div>
        <button
          @click="handleLogout"
          class="p-2 text-slate-400 hover:text-rose-600 hover:bg-rose-50 dark:hover:bg-rose-950/40 rounded-lg transition-colors ml-1"
          title="Đăng xuất"
        >
          <LogOut class="w-4 h-4" />
        </button>
      </div>
    </div>
  </header>
</template>
