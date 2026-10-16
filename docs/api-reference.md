# Tài Liệu Đặc Tả API Hệ Thống HospitalMS (API Reference)

Tất cả các API trong hệ thống đều tuân thủ chuẩn RESTful, phiên bản `/api/v1`, định dạng dữ liệu JSON, và cấu trúc phản hồi chuẩn mực:

```json
{
  "success": true,
  "data": { ... },
  "errors": null,
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
}
```

---

## 1. Identity Service (Cổng :5101 / YARP: `/identity/api/v1`)

### 1.1 Xác thực & Phân quyền
- **POST** `/api/v1/auth/login`
  - Body: `{ "username": "admin", "password": "Admin@123" }`
  - Response: `{ accessToken, refreshToken, expiresIn, user: { id, username, fullName, roles, permissions } }`
- **POST** `/api/v1/auth/refresh`
  - Body: `{ "refreshToken": "..." }`
- **GET** `/api/v1/me/applications`
  - Headers: `Authorization: Bearer <token>`
  - Response: Danh sách phân hệ mà người dùng có quyền mở (`HEALTH_EXAM`, `OUTPATIENT`, `INPATIENT`, `PHARMACY`, `ADMIN`).

### 1.2 Quản trị người dùng & vai trò
- **GET / POST** `/api/v1/users` (CRUD tài khoản)
- **GET / POST** `/api/v1/roles` (CRUD vai trò & gán quyền permissions)

---

## 2. Patient Service - MPI (Cổng :5102 / YARP: `/patient/api/v1`)

### Quản lý hồ sơ bệnh nhân
- **GET** `/api/v1/patients?searchTerm=BN&pageNumber=1&pageSize=20` (Tìm kiếm phân trang theo Mã BN, CCCD, BHYT, SĐT, Tên)
- **GET** `/api/v1/patients/{id}` (Lấy chi tiết hồ sơ bệnh nhân)
- **POST** `/api/v1/patients` (Tạo mới hồ sơ, tự sinh mã định danh duy nhất)
- **PUT** `/api/v1/patients/{id}` (Cập nhật thông tin hành chính, địa chỉ, BHYT)
- **POST** `/api/v1/patients/merge`
  - Body: `{ "sourcePatientId": "...", "targetPatientId": "...", "reason": "Trùng CCCD" }`
  - Chức năng: Gộp 2 hồ sơ bệnh nhân trùng lặp và ghi vết lịch sử.

---

## 3. Catalog Service (Cổng :5103 / YARP: `/catalog/api/v1`)

### Danh mục dùng chung
- **GET** `/api/v1/catalog/icd10?keyword=A00` (Tra cứu mã chẩn đoán ICD-10)
- **GET** `/api/v1/catalog/services?group=CLS` (Danh mục dịch vụ kỹ thuật, xét nghiệm, CĐHA)
- **GET** `/api/v1/catalog/departments` (Danh mục khoa phòng, buồng điều trị)

---

## 4. Outpatient Service (Cổng :5104 / YARP: `/outpatient/api/v1`)

### Khám bệnh & Viện phí ngoại trú
- **GET** `/api/v1/outpatient/queue?roomId={roomId}` (Hàng chờ tiếp đón phòng khám)
- **POST** `/api/v1/outpatient/queue/checkin` (Tiếp đón người bệnh vào hàng chờ)
- **POST** `/api/v1/outpatient/encounters/start` (Bác sĩ bắt đầu ca khám)
- **POST** `/api/v1/outpatient/encounters/{id}/diagnose`
  - Body: `{ "mainIcdCode": "I10", "mainDisease": "Tăng huyết áp vô căn", "temperature": 37.0, "bloodPressure": "120/80" }`
- **POST** `/api/v1/outpatient/encounters/{id}/orders` (Chỉ định dịch vụ cận lâm sàng)
- **POST** `/api/v1/outpatient/encounters/{id}/prescribe` (Kê đơn thuốc ngoại trú)
- **GET** `/api/v1/outpatient/encounters/{id}/billing` (Bảng kê viện phí & chi phí BHYT chi trả)
- **POST** `/api/v1/outpatient/encounters/{id}/complete` (Kết thúc ca khám và in đơn thuốc)

---

## 5. Inpatient Service (Cổng :5105 / YARP: `/inpatient/api/v1`)

### Quản lý nội trú & Sơ đồ giường bệnh
- **POST** `/api/v1/inpatient/admissions` (Tiếp nhận nhập viện nội trú)
- **GET** `/api/v1/inpatient/beds?departmentCode={dept}` (Lấy sơ đồ buồng/giường theo khoa)
- **PUT** `/api/v1/inpatient/beds/{bedId}/status` (Cập nhật trạng thái: `EMPTY`, `OCCUPIED`, `CLEANING`, `MAINTENANCE`)
- **POST** `/api/v1/inpatient/admissions/transfer-bed`
  - Body: `{ "admissionId": "...", "fromBedId": "...", "toBedId": "...", "reason": "Yêu cầu phòng cách ly" }`
- **POST** `/api/v1/inpatient/admissions/{id}/medical-orders` (Bác sĩ ra y lệnh điều trị/thuốc/chăm sóc)
- **POST** `/api/v1/inpatient/admissions/{id}/vitals` (Điều dưỡng cập nhật mạch, nhiệt, huyết áp)
- **POST** `/api/v1/inpatient/admissions/{id}/care-sheets` (Phiếu theo dõi chăm sóc)
- **POST** `/api/v1/inpatient/admissions/{id}/advances` (Thu tiền tạm ứng viện phí)
- **POST** `/api/v1/inpatient/admissions/{id}/discharge` (Tổng kết bệnh án & xuất viện, giải phóng giường)

---

## 6. Pharmacy Service (Cổng :5106 / YARP: `/pharmacy/api/v1`)

### Kho Dược, Tồn kho & Cấp phát
- **GET** `/api/v1/pharmacy/medicines?keyword=Paracetamol` (Tra cứu danh mục thuốc, hoạt chất, biệt dược)
- **GET** `/api/v1/pharmacy/warehouses` (Danh sách kho chính, kho lẻ, tủ trực)
- **GET** `/api/v1/pharmacy/stocks?warehouseId={id}` (Tồn kho theo lô, hạn dùng, cảnh báo cận date)
- **POST** `/api/v1/pharmacy/receipts` (Tạo phiếu nhập kho từ nhà cung cấp)
- **POST** `/api/v1/pharmacy/issues` (Tạo phiếu xuất kho/chuyển kho)
- **POST** `/api/v1/pharmacy/dispense`
  - Body: `{ "prescriptionId": "...", "warehouseId": "...", "items": [...] }`
  - Logic: Tự động trừ tồn kho theo FEFO (First Expired, First Out) với Optimistic Concurrency check.
- **GET** `/api/v1/pharmacy/stock-ledger?medicineId={id}` (Truy vấn sổ thẻ kho kiểm toán)

---

## 7. Health Exam Service (Cổng :5107 / YARP: `/healthexam/api/v1`)

### Khám sức khỏe doanh nghiệp / Hợp đồng đoàn
- **GET / POST** `/api/v1/healthexam/companies` (Quản lý doanh nghiệp, công ty đối tác)
- **GET / POST** `/api/v1/healthexam/contracts` (Hợp đồng khám đoàn, dự toán, thanh toán)
- **GET / POST** `/api/v1/healthexam/packages` (Gói khám sức khỏe, định mức dịch vụ)
- **POST** `/api/v1/healthexam/registrations/import-excel` (Import danh sách nhân viên từ Excel có validation từng dòng)
- **POST** `/api/v1/healthexam/registrations/{id}/checkin` (Check-in, cấp mã vạch, số thứ tự khám)
- **POST** `/api/v1/healthexam/visits/{visitId}/station-results` (Bác sĩ chuyên khoa nhập kết quả trạm khám)
- **POST** `/api/v1/healthexam/visits/{visitId}/conclude` (Bác sĩ trưởng đoàn phân loại sức khỏe loại 1->5, kết luận)
- **GET** `/api/v1/healthexam/visits/{visitId}/export-pdf` (Xuất PDF sổ khám / giấy khám sức khỏe)

