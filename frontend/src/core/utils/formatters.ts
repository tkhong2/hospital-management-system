/**
 * Các hàm format dữ liệu chuẩn y tế Việt Nam
 */

export function formatVND(amount: number | null | undefined): string {
  if (amount === null || amount === undefined) return '0 ₫'
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0
  }).format(amount)
}

export function formatDateVN(date: string | Date | null | undefined): string {
  if (!date) return '-'
  const d = new Date(date)
  if (isNaN(d.getTime())) return '-'
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  }).format(d)
}

export function formatDateTimeVN(date: string | Date | null | undefined): string {
  if (!date) return '-'
  const d = new Date(date)
  if (isNaN(d.getTime())) return '-'
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(d)
}

export function formatCitizenId(cccd: string | null | undefined): string {
  if (!cccd || cccd.length !== 12) return cccd || '-'
  return `${cccd.slice(0, 3)} ${cccd.slice(3, 6)} ${cccd.slice(6)}`
}

export function formatHealthInsurance(bhyt: string | null | undefined): string {
  if (!bhyt || bhyt.length !== 15) return bhyt || '-'
  return `${bhyt.slice(0, 2)} ${bhyt.slice(2, 3)} ${bhyt.slice(3, 5)} ${bhyt.slice(5, 7)} ${bhyt.slice(7)}`
}

export function getTT14Badge(type: number): { label: string; color: string } {
  switch (type) {
    case 1:
      return { label: 'Loại I (Rất khỏe)', color: 'bg-emerald-100 text-emerald-800 border-emerald-200' }
    case 2:
      return { label: 'Loại II (Khỏe)', color: 'bg-blue-100 text-blue-800 border-blue-200' }
    case 3:
      return { label: 'Loại III (Trung bình)', color: 'bg-amber-100 text-amber-800 border-amber-200' }
    case 4:
      return { label: 'Loại IV (Yếu)', color: 'bg-orange-100 text-orange-800 border-orange-200' }
    case 5:
      return { label: 'Loại V (Rất yếu)', color: 'bg-rose-100 text-rose-800 border-rose-200' }
    case 10:
      return { label: 'Đạt (Lái xe TT24)', color: 'bg-emerald-100 text-emerald-800 border-emerald-200' }
    case 11:
      return { label: 'Không đạt (Lái xe TT24)', color: 'bg-rose-100 text-rose-800 border-rose-200' }
    default:
      return { label: 'Chưa kết luận', color: 'bg-slate-100 text-slate-700 border-slate-200' }
  }
}
