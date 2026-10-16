import { ref, onMounted } from 'vue'
import { contractApi } from '../api/contractApi'
import type { HealthExamContract } from '../types/contract'

export function useContracts() {
  const contracts = ref<HealthExamContract[]>([])
  const totalCount = ref(0)
  const pageNumber = ref(1)
  const pageSize = ref(10)
  const loading = ref(false)
  const searchQuery = ref('')
  const error = ref<string | null>(null)

  async function fetchContracts() {
    loading.value = true
    error.value = null
    try {
      const response = await contractApi.getContracts({
        pageNumber: pageNumber.value,
        pageSize: pageSize.value,
        search: searchQuery.value || undefined
      })

      if (response.success && response.data) {
        contracts.value = response.data.items
        totalCount.value = response.data.totalCount
      }
    } catch (err: any) {
      error.value = err.message || 'Không thể tải danh sách hợp đồng'
      // Fallback mock data for immediate UI rendering if backend is offline
      contracts.value = [
        {
          id: '1',
          contractNumber: 'HĐ-2026/01-ABC',
          companyName: 'Công ty Cổ phần Công nghệ ABC',
          taxCode: '0102030405',
          representativeName: 'Nguyễn Văn A',
          phoneNumber: '0901234567',
          startDate: '2026-10-16',
          endDate: '2026-10-23',
          isMobileExam: false,
          estimatedPatientCount: 250,
          totalContractAmount: 375000000,
          totalPaidAmount: 150000000,
          remainingDebt: 225000000,
          status: 'Đang khám'
        },
        {
          id: '2',
          contractNumber: 'HĐ-2026/02-VNP',
          companyName: 'Tập đoàn Dược phẩm Vinapharm',
          taxCode: '0109988776',
          representativeName: 'Lê Thị B',
          phoneNumber: '0912345678',
          startDate: '2026-10-18',
          endDate: '2026-10-28',
          isMobileExam: true,
          estimatedPatientCount: 120,
          totalContractAmount: 240000000,
          totalPaidAmount: 240000000,
          remainingDebt: 0,
          status: 'Chờ khám'
        },
        {
          id: '3',
          contractNumber: 'HĐ-2026/03-LOG',
          companyName: 'Công ty Vận tải & Logistics Miền Đông',
          taxCode: '0301122334',
          representativeName: 'Phạm Quốc C',
          phoneNumber: '0987654321',
          startDate: '2026-10-20',
          endDate: '2026-10-27',
          isMobileExam: false,
          estimatedPatientCount: 85,
          totalContractAmount: 102000000,
          totalPaidAmount: 50000000,
          remainingDebt: 52000000,
          status: 'Lên lịch'
        }
      ]
      totalCount.value = contracts.value.length
    } finally {
      loading.value = false
    }
  }

  onMounted(() => {
    fetchContracts()
  })

  return {
    contracts,
    totalCount,
    pageNumber,
    pageSize,
    loading,
    searchQuery,
    error,
    fetchContracts
  }
}
