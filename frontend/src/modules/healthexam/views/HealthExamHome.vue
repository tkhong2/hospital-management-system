<script setup lang="ts">
import { ref } from 'vue'
import {
  HeartPulse,
  Activity,
  Plus,
  Upload,
  Search,
  Building2,
  DollarSign,
  Users
} from 'lucide-vue-next'
import { useContracts } from '../composables/useContracts'
import { formatVND, formatDateVN } from '@/core/utils/formatters'
import StatusBadge from '@/core/components/StatusBadge.vue'

const {
  contracts,
  loading,
  searchQuery,
  fetchContracts
} = useContracts()

const examinationStations = [
  { id: 'vital', name: 'Thể lực & Dấu sinh tồn', status: '8 chờ', color: 'text-blue-600' },
  { id: 'internal', name: 'Nội tổng quát', status: '12 chờ', color: 'text-indigo-600' },
  { id: 'surgery', name: 'Ngoại khoa & Da liễu', status: '4 chờ', color: 'text-violet-600' },
  { id: 'ent', name: 'Tai Mũi Họng', status: '6 chờ', color: 'text-teal-600' },
  { id: 'dental', name: 'Răng Hàm Mặt', status: '5 chờ', color: 'text-cyan-600' },
  { id: 'eye', name: 'Mắt & Đo thị lực', status: '9 chờ', color: 'text-sky-600' },
  { id: 'lab', name: 'Xét nghiệm (Huyết học/Nước tiểu)', status: '15 chờ', color: 'text-amber-600' },
  { id: 'imaging', name: 'CĐHA (X-Quang, Siêu âm)', status: '11 chờ', color: 'text-orange-600' },
  { id: 'conclusion', name: 'Bác sĩ Kết luận TT14/TT24', status: '3 chờ', color: 'text-emerald-600' },
]

function getStatusVariant(status: string): 'success' | 'warning' | 'info' | 'default' {
  if (status.includes('Đang khám')) return 'info'
  if (status.includes('Chờ khám')) return 'warning'
  if (status.includes('Hoàn tất')) return 'success'
  return 'default'
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center text-white shadow-md">
          <HeartPulse class="w-6 h-6" />
        </div>
        <div>
          <div class="flex items-center gap-2">
            <h1 class="text-xl font-bold text-slate-900">Phân hệ Khám Sức Khỏe (HealthExam)</h1>
            <span class="px-2.5 py-0.5 text-xs font-semibold bg-emerald-100 text-emerald-800 rounded-full">Trọng tâm</span>
          </div>
          <p class="text-xs text-slate-500 mt-0.5">Quản lý hợp đồng đoàn, gói khám, quy trình trạm đa chuyên khoa, kết luận Thông tư 14/2013 & 24/2015</p>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <button class="inline-flex items-center gap-1.5 px-3 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-semibold rounded-lg transition-colors">
          <Upload class="w-4 h-4" />
          <span>Import Excel Đoàn</span>
        </button>
        <button class="inline-flex items-center gap-1.5 px-3 py-2 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-semibold rounded-lg shadow-sm transition-colors">
          <Plus class="w-4 h-4" />
          <span>Tạo Hợp Đồng Mới</span>
        </button>
      </div>
    </div>

    <!-- Multi-Specialty Stations Flow -->
    <div class="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-sm font-bold text-slate-800 uppercase tracking-wider flex items-center gap-2">
          <Activity class="w-4 h-4 text-emerald-600" />
          <span>Tiến Độ Khám Đa Chuyên Khoa Theo Trạm (Station Workflow)</span>
        </h2>
        <span class="text-xs text-slate-400">Giám sát tải phòng khám</span>
      </div>

      <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3">
        <div
          v-for="station in examinationStations"
          :key="station.id"
          class="p-3.5 rounded-xl border border-slate-100 bg-slate-50/70 hover:bg-emerald-50/40 hover:border-emerald-300 transition-all text-center cursor-pointer group"
        >
          <div class="text-xs font-semibold text-slate-800 group-hover:text-emerald-700 transition-colors">
            {{ station.name }}
          </div>
          <div :class="['text-[11px] font-bold mt-1.5', station.color]">
            {{ station.status }}
          </div>
        </div>
      </div>
    </div>

    <!-- Contracts Table -->
    <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
      <div class="p-4 border-b border-slate-200 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div class="relative flex-1 max-w-sm">
          <Search class="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
          <input
            v-model="searchQuery"
            @input="fetchContracts"
            type="text"
            placeholder="Tìm theo tên công ty, mã hợp đồng..."
            class="w-full pl-9 pr-4 py-1.5 text-xs rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500"
          />
        </div>
        <div class="text-xs text-slate-500">
          Tổng cộng: <strong class="text-slate-800">{{ contracts.length }}</strong> hợp đồng
        </div>
      </div>

      <div class="overflow-x-auto">
        <table class="w-full text-left text-xs">
          <thead class="bg-slate-50 text-slate-500 uppercase font-semibold border-b border-slate-200">
            <tr>
              <th class="p-3.5">Mã Hợp Đồng</th>
              <th class="p-3.5">Công ty / Cơ quan</th>
              <th class="p-3.5">Quy mô đoàn</th>
              <th class="p-3.5">Giá trị hợp đồng</th>
              <th class="p-3.5">Còn nợ</th>
              <th class="p-3.5">Thời gian khám</th>
              <th class="p-3.5">Trạng thái</th>
              <th class="p-3.5 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="loading">
              <td colspan="8" class="p-8 text-center text-slate-400">Đang tải dữ liệu...</td>
            </tr>
            <tr v-else v-for="c in contracts" :key="c.id" class="hover:bg-slate-50/80 transition-colors">
              <td class="p-3.5 font-bold text-slate-800">{{ c.contractNumber }}</td>
              <td class="p-3.5 font-medium text-slate-900">
                <div>{{ c.companyName }}</div>
                <div class="text-[10px] text-slate-400">MST: {{ c.taxCode }}</div>
              </td>
              <td class="p-3.5 font-semibold text-slate-800">
                <span class="inline-flex items-center gap-1">
                  <Users class="w-3.5 h-3.5 text-slate-400" />
                  {{ c.estimatedPatientCount }} người
                </span>
                <div v-if="c.isMobileExam" class="text-[10px] text-teal-600 font-medium">Khám lưu động</div>
              </td>
              <td class="p-3.5 font-semibold text-slate-900">{{ formatVND(c.totalContractAmount) }}</td>
              <td class="p-3.5 font-semibold text-rose-600">
                {{ c.remainingDebt > 0 ? formatVND(c.remainingDebt) : 'Đã tất toán' }}
              </td>
              <td class="p-3.5 text-slate-500">{{ formatDateVN(c.startDate) }} - {{ formatDateVN(c.endDate) }}</td>
              <td class="p-3.5">
                <StatusBadge :status="c.status" :variant="getStatusVariant(c.status)" />
              </td>
              <td class="p-3.5 text-right space-x-2">
                <button class="text-emerald-600 hover:text-emerald-800 font-semibold">Xem chi tiết</button>
                <button class="text-slate-400 hover:text-slate-600">In danh sách</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
