<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import {
  Search,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  ArrowUpDown,
  ArrowUp,
  ArrowDown,
  Download,
  Loader2,
  FolderOpen
} from 'lucide-vue-next'

export interface ColumnDefinition {
  key: string
  label: string
  sortable?: boolean
  width?: string
  align?: 'left' | 'center' | 'right'
}

const props = withDefaults(
  defineProps<{
    columns: ColumnDefinition[]
    items: any[]
    loading?: boolean
    totalCount?: number
    page?: number
    pageSize?: number
    searchable?: boolean
    searchPlaceholder?: string
    exportable?: boolean
    exportFileName?: string
    emptyText?: string
  }>(),
  {
    loading: false,
    totalCount: 0,
    page: 1,
    pageSize: 10,
    searchable: true,
    searchPlaceholder: 'Tìm kiếm nhanh...',
    exportable: true,
    exportFileName: 'danh-sach',
    emptyText: 'Không tìm thấy dữ liệu phù hợp'
  }
)

const emit = defineEmits<{
  (e: 'update:page', page: number): void
  (e: 'update:pageSize', pageSize: number): void
  (e: 'update:search', search: string): void
  (e: 'sort', payload: { sortBy: string; sortDescending: boolean }): void
  (e: 'rowClick', item: any): void
}>()

const searchValue = ref('')
const currentSortBy = ref<string | null>(null)
const currentSortDesc = ref(false)

let debounceTimer: ReturnType<typeof setTimeout> | null = null

const onSearchInput = () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    emit('update:search', searchValue.value.trim())
  }, 400)
}

const handleSort = (column: ColumnDefinition) => {
  if (!column.sortable) return
  if (currentSortBy.value === column.key) {
    if (currentSortDesc.value) {
      currentSortBy.value = null
      currentSortDesc.value = false
    } else {
      currentSortDesc.value = true
    }
  } else {
    currentSortBy.value = column.key
    currentSortDesc.value = false
  }

  emit('sort', {
    sortBy: currentSortBy.value || '',
    sortDescending: currentSortDesc.value
  })
}

const totalPages = computed(() => {
  if (props.totalCount <= 0) return 1
  return Math.ceil(props.totalCount / props.pageSize)
})

const startRecord = computed(() => {
  if (props.totalCount === 0) return 0
  return (props.page - 1) * props.pageSize + 1
})

const endRecord = computed(() => {
  const end = props.page * props.pageSize
  return end > props.totalCount ? props.totalCount : end
})

const changePage = (newPage: number) => {
  if (newPage >= 1 && newPage <= totalPages.value && newPage !== props.page) {
    emit('update:page', newPage)
  }
}

const onPageSizeChange = (event: Event) => {
  const target = event.target as HTMLSelectElement
  emit('update:pageSize', Number(target.value))
}

const exportToCsv = () => {
  if (!props.items || props.items.length === 0) return

  const headers = props.columns.map((c) => `"${c.label.replace(/"/g, '""')}"`).join(',')
  const rows = props.items.map((item) => {
    return props.columns
      .map((col) => {
        const val = item[col.key]
        if (val === null || val === undefined) return '""'
        return `"${String(val).replace(/"/g, '""')}"`
      })
      .join(',')
  })

  // UTF-8 BOM for Excel compatibility with Vietnamese
  const csvContent = '\uFEFF' + [headers, ...rows].join('\r\n')
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.setAttribute('href', url)
  link.setAttribute('download', `${props.exportFileName}_${new Date().toISOString().slice(0, 10)}.csv`)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
}
</script>

<template>
  <div class="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col">
    <!-- Toolbar: Search & Actions -->
    <div class="p-4 border-b border-slate-100 flex flex-wrap items-center justify-between gap-4 bg-slate-50/50">
      <div class="flex items-center gap-3 flex-1 min-w-[240px] max-w-md">
        <div v-if="searchable" class="relative w-full">
          <Search class="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <input
            v-model="searchValue"
            type="text"
            :placeholder="searchPlaceholder"
            @input="onSearchInput"
            class="w-full pl-10 pr-4 py-2 text-sm bg-white border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all text-slate-700 placeholder-slate-400 shadow-xs"
          />
        </div>
        <slot name="filters" />
      </div>

      <div class="flex items-center gap-2">
        <slot name="extra-actions" />
        <button
          v-if="exportable"
          @click="exportToCsv"
          :disabled="items.length === 0"
          type="button"
          class="inline-flex items-center gap-1.5 px-3 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-200 rounded-xl hover:bg-slate-50 hover:text-slate-900 transition-colors shadow-xs disabled:opacity-40 disabled:cursor-not-allowed"
          title="Xuất file CSV/Excel"
        >
          <Download class="w-3.5 h-3.5 text-slate-500" />
          <span>Xuất CSV</span>
        </button>
      </div>
    </div>

    <!-- Table Container -->
    <div class="overflow-x-auto relative min-h-[220px]">
      <table class="w-full text-left border-collapse">
        <thead>
          <tr class="border-b border-slate-100 bg-slate-50/80 text-xs font-semibold text-slate-600 uppercase tracking-wider">
            <th
              v-for="col in columns"
              :key="col.key"
              :style="{ width: col.width }"
              :class="[
                'py-3.5 px-4 select-none',
                col.align === 'center' ? 'text-center' : col.align === 'right' ? 'text-right' : 'text-left',
                col.sortable ? 'cursor-pointer hover:bg-slate-100/80 transition-colors' : ''
              ]"
              @click="handleSort(col)"
            >
              <div
                class="inline-flex items-center gap-1.5"
                :class="[col.align === 'center' ? 'justify-center' : col.align === 'right' ? 'justify-end' : 'justify-start']"
              >
                <span>{{ col.label }}</span>
                <span v-if="col.sortable" class="text-slate-400">
                  <ArrowUp v-if="currentSortBy === col.key && !currentSortDesc" class="w-3.5 h-3.5 text-primary" />
                  <ArrowDown v-else-if="currentSortBy === col.key && currentSortDesc" class="w-3.5 h-3.5 text-primary" />
                  <ArrowUpDown v-else class="w-3 h-3 opacity-40 hover:opacity-100" />
                </span>
              </div>
            </th>
            <th v-if="$slots.actions" class="py-3.5 px-4 text-right text-xs font-semibold text-slate-600 uppercase tracking-wider w-28">
              Thao tác
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-100 text-sm text-slate-700">
          <!-- Loading Overlay -->
          <tr v-if="loading">
            <td :colspan="columns.length + ($slots.actions ? 1 : 0)" class="py-16 text-center text-slate-400">
              <div class="flex flex-col items-center justify-center gap-2">
                <Loader2 class="w-8 h-8 animate-spin text-primary" />
                <span class="text-xs font-medium">Đang tải dữ liệu...</span>
              </div>
            </td>
          </tr>

          <!-- Empty State -->
          <tr v-else-if="items.length === 0">
            <td :colspan="columns.length + ($slots.actions ? 1 : 0)" class="py-16 text-center text-slate-400">
              <div class="flex flex-col items-center justify-center gap-2">
                <FolderOpen class="w-10 h-10 text-slate-300 stroke-1" />
                <span class="text-xs font-medium">{{ emptyText }}</span>
              </div>
            </td>
          </tr>

          <!-- Data Rows -->
          <tr
            v-else
            v-for="(item, idx) in items"
            :key="item.id || idx"
            @click="emit('rowClick', item)"
            class="hover:bg-blue-50/40 transition-colors cursor-pointer group"
          >
            <td
              v-for="col in columns"
              :key="col.key"
              :class="[
                'py-3.5 px-4 align-middle',
                col.align === 'center' ? 'text-center' : col.align === 'right' ? 'text-right' : 'text-left'
              ]"
            >
              <slot :name="`cell-${col.key}`" :item="item" :value="item[col.key]">
                <span class="font-normal">{{ item[col.key] ?? '—' }}</span>
              </slot>
            </td>
            <td v-if="$slots.actions" class="py-3.5 px-4 text-right align-middle" @click.stop>
              <slot name="actions" :item="item" />
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination Footer -->
    <div class="p-3.5 px-4 border-t border-slate-100 flex flex-wrap items-center justify-between gap-4 bg-slate-50/60 text-xs text-slate-500">
      <div class="flex items-center gap-2">
        <span>Hiển thị</span>
        <select
          :value="pageSize"
          @change="onPageSizeChange"
          class="border border-slate-200 rounded-lg px-2 py-1 bg-white text-slate-700 font-medium focus:outline-none focus:ring-1 focus:ring-primary shadow-xs"
        >
          <option :value="10">10</option>
          <option :value="25">25</option>
          <option :value="50">50</option>
          <option :value="100">100</option>
        </select>
        <span>bản ghi / trang</span>
        <span class="text-slate-300 mx-1">|</span>
        <span class="font-medium text-slate-700">
          {{ startRecord }} - {{ endRecord }} trong tổng số {{ totalCount }}
        </span>
      </div>

      <div class="flex items-center gap-1">
        <button
          @click="changePage(1)"
          :disabled="page <= 1"
          class="p-1.5 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
          title="Trang đầu"
        >
          <ChevronsLeft class="w-4 h-4" />
        </button>
        <button
          @click="changePage(page - 1)"
          :disabled="page <= 1"
          class="p-1.5 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
          title="Trang trước"
        >
          <ChevronLeft class="w-4 h-4" />
        </button>

        <span class="px-3 py-1 font-semibold text-slate-700">
          {{ page }} / {{ totalPages }}
        </span>

        <button
          @click="changePage(page + 1)"
          :disabled="page >= totalPages"
          class="p-1.5 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
          title="Trang sau"
        >
          <ChevronRight class="w-4 h-4" />
        </button>
        <button
          @click="changePage(totalPages)"
          :disabled="page >= totalPages"
          class="p-1.5 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
          title="Trang cuối"
        >
          <ChevronsRight class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>
</template>

