<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { Stethoscope, Lock, User, ArrowRight, ShieldCheck } from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()

const username = ref('admin')
const password = ref('admin123')
const loading = ref(false)
const error = ref('')

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
    <div class="max-w-md w-full bg-white/95 backdrop-blur-md rounded-2xl shadow-2xl p-8 border border-white/20">
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-br from-hospital-500 to-hospital-700 text-white shadow-lg mb-4">
          <Stethoscope class="w-8 h-8" />
        </div>
        <h1 class="text-2xl font-bold text-slate-900 tracking-tight">Hệ thống Quản lý Bệnh viện</h1>
        <p class="text-sm text-slate-500 mt-1">Đăng nhập tài khoản cán bộ y tế & quản trị</p>
      </div>

      <div v-if="error" class="mb-4 p-3 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-xs">
        {{ error }}
      </div>

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1.5 uppercase tracking-wider">Tên đăng nhập</label>
          <div class="relative">
            <User class="w-4 h-4 text-slate-400 absolute left-3.5 top-3" />
            <input
              v-model="username"
              type="text"
              class="w-full pl-10 pr-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-hospital-500 text-sm"
              placeholder="Nhập tên đăng nhập hoặc mã cán bộ"
              required
            />
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1.5 uppercase tracking-wider">Mật khẩu</label>
          <div class="relative">
            <Lock class="w-4 h-4 text-slate-400 absolute left-3.5 top-3" />
            <input
              v-model="password"
              type="password"
              class="w-full pl-10 pr-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-hospital-500 text-sm"
              placeholder="Nhập mật khẩu"
              required
            />
          </div>
        </div>

        <div class="flex items-center justify-between text-xs pt-1">
          <label class="flex items-center text-slate-600 cursor-pointer">
            <input type="checkbox" checked class="rounded border-slate-300 text-hospital-600 focus:ring-hospital-500 mr-2" />
            Ghi nhớ đăng nhập
          </label>
          <a href="#" class="text-hospital-600 hover:underline">Quên mật khẩu?</a>
        </div>

        <button
          type="submit"
          :disabled="loading"
          class="w-full mt-2 py-3 px-4 bg-gradient-to-r from-hospital-600 to-hospital-700 hover:from-hospital-700 hover:to-hospital-800 text-white font-semibold rounded-lg shadow-md hover:shadow-lg transition-all flex items-center justify-center gap-2 text-sm disabled:opacity-50"
        >
          <span>{{ loading ? 'Đang xác thực...' : 'Đăng nhập vào Hệ thống' }}</span>
          <ArrowRight class="w-4 h-4" />
        </button>
      </form>

      <div class="mt-8 pt-6 border-t border-slate-100 flex items-center justify-center gap-2 text-xs text-slate-400">
        <ShieldCheck class="w-4 h-4 text-emerald-600" />
        <span>Bảo mật chuẩn y tế & SSO Identity Service</span>
      </div>
    </div>
  </div>
</template>

