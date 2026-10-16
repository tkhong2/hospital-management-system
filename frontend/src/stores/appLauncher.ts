import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { HospitalApp } from '@/types/app'

export const useAppLauncherStore = defineStore('appLauncher', () => {
  const isLauncherOpen = ref(false)

  const apps = ref<HospitalApp[]>([
    {
      id: 'healthexam',
      code: 'healthexam',
      name: 'Khám sức khỏe',
      description: 'Hợp đồng đoàn, gói khám, quy trình trạm đa chuyên khoa, kết luận TT14/TT24',
      icon: 'HeartPulse',
      color: 'from-emerald-500 to-teal-600',
      path: '/healthexam',
      badge: 'Trọng tâm'
    },
    {
      id: 'outpatient',
      code: 'outpatient',
      name: 'Khám ngoại trú',
      description: 'Tiếp đón, buồng khám, chỉ định cận lâm sàng, kê đơn điện tử, viện phí',
      icon: 'Stethoscope',
      color: 'from-blue-500 to-indigo-600',
      path: '/outpatient'
    },
    {
      id: 'inpatient',
      code: 'inpatient',
      name: 'Quản lý nội trú',
      description: 'Nhập viện, sơ đồ giường, chuyển khoa, y lệnh điều trị, hồ sơ bệnh án',
      icon: 'BedDouble',
      color: 'from-violet-500 to-purple-600',
      path: '/inpatient'
    },
    {
      id: 'pharmacy',
      code: 'pharmacy',
      name: 'Kho dược & Vật tư',
      description: 'Quản lý nhập xuất tồn, lô hạn dùng (FEFO), duyệt đơn, cấp phát thuốc',
      icon: 'Pill',
      color: 'from-amber-500 to-orange-600',
      path: '/pharmacy'
    },
    {
      id: 'patient',
      code: 'patient',
      name: 'Hồ sơ Bệnh nhân (MPI)',
      description: 'Mã bệnh nhân duy nhất, CCCD, thẻ BHYT và lịch sử khám chữa bệnh',
      icon: 'Users',
      color: 'from-cyan-500 to-blue-600',
      path: '/patient'
    },
    {
      id: 'catalog',
      code: 'catalog',
      name: 'Danh mục dùng chung',
      description: 'ICD-10, dịch vụ kỹ thuật TT39, khoa phòng, phân loại thuốc',
      icon: 'Database',
      color: 'from-slate-600 to-slate-800',
      path: '/catalog'
    },
    {
      id: 'admin',
      code: 'admin',
      name: 'Quản trị hệ thống',
      description: 'Tài khoản, vai trò, ma trận quyền RBAC, cấu hình hệ thống',
      icon: 'ShieldCheck',
      color: 'from-rose-500 to-red-600',
      path: '/admin'
    }
  ])

  function toggleLauncher(): void {
    isLauncherOpen.value = !isLauncherOpen.value
  }

  function closeLauncher(): void {
    isLauncherOpen.value = false
  }

  return {
    apps,
    isLauncherOpen,
    toggleLauncher,
    closeLauncher
  }
})

