<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { Stethoscope, Lock, User, ArrowRight, ShieldCheck, Sparkles } from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()

const username = ref('admin')
const password = ref('Admin@123')
const loading = ref(false)
const error = ref('')

const demoAccounts = [
  { role: 'Quản trị viên', user: 'admin', pass: 'Admin@123', color: 'bg-rose-50 dark:bg-rose-950/40 text-rose-700 dark:text-rose-300 border-rose-200 dark:border-rose-800' },
  { role: 'Bác sĩ Khám SK', user: 'doctor.an', pass: 'Doctor@123', color: 'bg-emerald-50 dark:bg-emerald-950/40 text-emerald-700 dark:text-emerald-300 border-emerald-200 dark:border-emerald-800' },
  { role: 'Dược sĩ Kho', user: 'pharmacist.mai', pass: 'Pharm@123', color: 'bg-amber-50 dark:bg-amber-950/40 text-amber-700 dark:text-amber-300 border-amber-200 dark:border-amber-800' },
  { role: 'Lễ tân Tiếp đón', user: 'reception.nga', pass: 'Recept@123', color: 'bg-blue-50 dark:bg-blue-950/40 text-blue-700 dark:text-blue-300 border-blue-200 dark:border-blue-800' }
]

function fillDemo(acc: { user: string; pass: string }) {
  username.value = acc.user
  password.value = acc.pass
  error.value = ''
}

async function handleSubmit() {
  if (!username.value || !password.value) {
    error.value = 'Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu'
    return
  }

  loading.value = true
  error.value = ''

  try {
    await authStore.login({
      username: username.value,
      password: password.value
    })
    router.push('/')
  } catch (err: any) {
    error.value = err.message || 'Đăng nhập không thành công. Vui lòng kiểm tra lại.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-gradient-to-br from-slate-900 via-slate-800 to-hospital-950 flex items-center justify-center p-4">
    <div class="max-w-md w-full bg-white/95 dark:bg-slate-900/95 backdrop-blur-md rounded-2xl shadow-2xl p-8 border border-white/20 dark:border-slate-800 transition-colors">
      <div class="text-center mb-6">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-br from-hospital-500 to-hospital-700 text-white shadow-lg mb-3">
          <Stethoscope class="w-8 h-8" />
        </div>
        <h1 class="text-2xl font-bold text-slate-900 dark:text-white tracking-tight">Hệ thống Quản lý Bệnh viện</h1>
        <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">Đăng nhập tài khoản định danh cán bộ y tế (SSO Identity)</p>
      </div>

      <!-- Quick Demo Account Switcher -->
      <div class="mb-5 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/80 border border-slate-200/70 dark:border-slate-700">
        <div class="flex items-center gap-1.5 text-[11px] font-semibold text-slate-600 dark:text-slate-300 mb-2">
          <Sparkles class="w-3.5 h-3.5 text-amber-500" />
          <span>Tài khoản mẫu thử nghiệm phân quyền:</span>
        </div>
        <div class="grid grid-cols-2 gap-1.5">
          <button
            v-for="(acc, i) in demoAccounts"
            :key="i"
            type="button"
            @click="fillDemo(acc)"
            :class="['text-[11px] px-2 py-1.5 rounded-lg border font-medium text-left truncate transition-colors hover:brightness-95', acc.color]"
          >
            {{ acc.role }}
          </button>
        </div>
      </div>

      <div v-if="error" class="mb-4 p-3 rounded-lg bg-rose-50 dark:bg-rose-950/50 border border-rose-200 dark:border-rose-900 text-rose-700 dark:text-rose-300 text-xs">
        {{ error }}
      </div>

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div>
          <label class="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1.5 uppercase tracking-wider">Tên đăng nhập</label>
          <div class="relative">
            <User class="w-4 h-4 text-slate-400 absolute left-3.5 top-3" />
            <input
              v-model="username"
              type="text"
              class="w-full pl-10 pr-4 py-2.5 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-hospital-500 text-sm"
              placeholder="Nhập username"
              required
            />
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 dark:text-slate-300 mb-1.5 uppercase tracking-wider">Mật khẩu</label>
          <div class="relative">
            <Lock class="w-4 h-4 text-slate-400 absolute left-3.5 top-3" />
            <input
              v-model="password"
              type="password"
              class="w-full pl-10 pr-4 py-2.5 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-hospital-500 text-sm"
              placeholder="Nhập mật khẩu"
              required
            />
          </div>
        </div>

        <div class="flex items-center justify-between text-xs pt-1">
          <label class="flex items-center text-slate-600 dark:text-slate-400 cursor-pointer">
            <input type="checkbox" checked class="rounded border-slate-300 text-hospital-600 focus:ring-hospital-500 mr-2" />
            Ghi nhớ phiên đăng nhập
          </label>
          <a href="#" class="text-hospital-600 dark:text-hospital-400 hover:underline">Quên mật khẩu?</a>
        </div>

        <button
          type="submit"
          :disabled="loading"
          class="w-full mt-2 py-3 px-4 bg-gradient-to-r from-hospital-600 to-hospital-700 hover:from-hospital-700 hover:to-hospital-800 text-white font-semibold rounded-lg shadow-md hover:shadow-lg transition-all flex items-center justify-center gap-2 text-sm disabled:opacity-50"
        >
          <span>{{ loading ? 'Đang xác thực bảo mật...' : 'Đăng nhập vào Hệ thống' }}</span>
          <ArrowRight class="w-4 h-4" />
        </button>
      </form>

      <div class="mt-6 pt-4 border-t border-slate-100 dark:border-slate-800 flex items-center justify-center gap-2 text-xs text-slate-400 dark:text-slate-500">
        <ShieldCheck class="w-4 h-4 text-emerald-600 dark:text-emerald-400" />
        <span>Bảo mật JWT & Token Rotation theo chuẩn Y tế</span>
      </div>
    </div>
  </div>
</template>
