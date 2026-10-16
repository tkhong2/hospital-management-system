# Hướng Dẫn Triển Khai Hệ Thống HospitalMS (Deployment Guide)

Tài liệu này hướng dẫn chi tiết quy trình triển khai và vận hành hệ thống **Hospital Management System (HospitalMS)** ở các môi trường Local Development, Docker Compose và Production.

---

## 1. Yêu Cầu Hạ Tầng & Phần Cứng

### Cấu hình tối thiểu (Development / Demo):
- **CPU:** 4 Cores (x86_64)
- **RAM:** 8 GB
- **Disk:** 30 GB SSD
- **Hệ điều hành:** Linux (Ubuntu 22.04+ / Debian 12) hoặc Windows 11 với WSL2 / macOS Sonoma+

### Cấu hình khuyến nghị (Production):
- **CPU:** 8 - 16 Cores
- **RAM:** 16 - 32 GB
- **Disk:** 100+ GB NVMe SSD
- **Môi trường runtime:** Docker Engine 24.0+ & Docker Compose v2.20+

---

## 2. Bảng Phân Bổ Cổng Mạng (Port Mapping)

| Dịch vụ | Protocol | Cổng Container | Cổng Host (Dev/Prod) | Mô tả |
| :--- | :--- | :--- | :--- | :--- |
| **Frontend Portal** | HTTP | 80 | **80** (Prod) / 5173 (Dev) | Vue 3 SPA qua Nginx |
| **API Gateway (YARP)** | HTTP | 8080 | **5100** | Reverse Proxy điều hướng API |
| **IdentityService** | HTTP | 8080 | **5101** | Xác thực, JWT, RBAC |
| **PatientService** | HTTP | 8080 | **5102** | Quản lý MPI bệnh nhân |
| **CatalogService** | HTTP | 8080 | **5103** | Danh mục ICD-10, dịch vụ |
| **OutpatientService** | HTTP | 8080 | **5104** | Khám bệnh ngoại trú, viện phí |
| **InpatientService** | HTTP | 8080 | **5105** | Nội trú, buồng giường |
| **PharmacyService** | HTTP | 8080 | **5106** | Kho dược, thẻ kho, cấp phát |
| **HealthExamService** | HTTP | 8080 | **5107** | Khám đoàn, gói khám, kết luận |
| **PostgreSQL** | TCP | 5432 | **5432** | 7 Cơ sở dữ liệu độc lập |
| **RabbitMQ** | TCP / HTTP | 5672 / 15672 | **5672 / 15672** | Message Broker & Dashboard |
| **Redis** | TCP | 6379 | **6379** | Distributed Cache |
| **Seq** | HTTP | 5341 / 80 | **5341 / 8081** | Centralized Logging Dashboard |
| **pgAdmin** | HTTP | 80 | **5050** | Quản trị CSDL giao diện web |

---

## 3. Triển Khai Nhanh Bằng Docker Compose

### Bước 1: Sao chép mã nguồn và thiết lập biến môi trường
```bash
git clone https://github.com/your-org/hospital-management-system.git
cd hospital-management-system
cp .env.example .env
```

### Bước 2: Khởi động toàn bộ cụm dịch vụ Production
```bash
docker compose -f docker-compose.prod.yml up -d --build
```

### Bước 3: Kiểm tra trạng thái sức khỏe các containers
```bash
docker compose -f docker-compose.prod.yml ps
```
Đảm bảo tất cả các container đều ở trạng thái `healthy` hoặc `Up`.

---

## 4. Chạy Từng Dịch Vụ Cục Bộ (Local Development)

### 4.1 Khởi động hạ tầng nền tảng (DB, RabbitMQ, Redis, Seq)
```bash
docker compose up -d postgres rabbitmq redis seq
```

### 4.2 Chạy Backend (.NET 9)
Khởi động API Gateway và các microservices:
```bash
cd backend
dotnet restore HospitalMS.sln
dotnet test HospitalMS.sln
dotnet run --project src/ApiGateway/HospitalMS.ApiGateway/HospitalMS.ApiGateway.csproj
```

### 4.3 Chạy Frontend (Vue 3 + Vite)
```bash
cd frontend
npm install
npm run dev
```
Truy cập giao diện tại: `http://localhost:5173`.

---

## 5. Tài Khoản Mặc Định Kiểm Thử

| Quyền hạn | Tên đăng nhập | Mật khẩu | Phạm vi truy cập |
| :--- | :--- | :--- | :--- |
| **Quản trị hệ thống (Admin)** | `admin` | `Admin@123` | Toàn quyền tất cả các phân hệ và cấu hình |
| **Bác sĩ khám bệnh** | `doctor` | `Doctor@123` | Khám ngoại trú, chỉ định CLS, kê đơn |
| **Bác sĩ nội trú** | `inpatient_doc` | `Doctor@123` | Buồng giường nội trú, y lệnh, xuất viện |
| **Dược sĩ kho** | `pharmacist` | `Pharm@123` | Kho dược, duyệt đơn thuốc, kiểm kê |
| **Lễ tân / Tiếp đón** | `receptionist` | `Recep@123` | Tiếp đón bệnh nhân, khám sức khỏe đoàn |

---

## 6. Giám Sát, Log & Truy Vết

- **Seq Log Server:** `http://localhost:8081` (Xem toàn bộ structured log, lọc theo `Application`, `UserId`, `TraceId`).
- **RabbitMQ UI:** `http://localhost:15672` (Username: `guest` / Password: `guest`).
- **Health Check endpoints:**
  - Liveness: `http://localhost:5100/health/live` (Process check)
  - Readiness: `http://localhost:5100/health/ready` (Postgres, RabbitMQ, Redis connectivity)
- **OpenTelemetry Tracing:** Kích hoạt qua `HospitalActivitySource` và export sang Jaeger/OTLP collector.

