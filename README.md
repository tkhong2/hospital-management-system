# Hệ Thống Quản Lý Bệnh Viện Tích Hợp (HospitalMS)

[![CI Pipeline](https://github.com/your-org/hospital-management-system/actions/workflows/ci.yml/badge.svg)](https://github.com/your-org/hospital-management-system/actions/workflows/ci.yml)
[![Tests Passing](https://img.shields.io/badge/Tests-60%2F60%20Passed-success.svg)](https://github.com/your-org/hospital-management-system)
[![TypeScript](https://img.shields.io/badge/TypeScript-Strict%200%20Errors-blue.svg)](https://github.com/your-org/hospital-management-system)
[![.NET 9](https://img.shields.io/badge/.NET-9.0%20LTS-purple.svg)](https://dotnet.microsoft.com/)

Hệ thống quản lý bệnh viện tích hợp chuyên sâu theo tiêu chuẩn y tế Việt Nam (Thông tư 14/2013/TT-BYT, Thông tư 24/2015/TTLT-BYT-BGTVT, ICD-10, BHYT) được thiết kế theo mô hình **Microservices Monorepo** hiện đại, phân tách rạch ròi `backend/` (.NET 9 Web API, Clean Architecture, CQRS) và `frontend/` (Vue 3, TypeScript, Vite, TailwindCSS, PrimeVue).

---

## 1. Cấu Trúc Tổng Thể Dự Án

```
hospital-management-system/
├── .github/
│   └── workflows/
│       └── ci.yml                          # CI Pipeline kiểm tra tự động Backend test + Frontend build
├── backend/                                # Toàn bộ mã nguồn & kiểm thử Backend .NET 9
│   ├── HospitalMS.sln                      # Solution gồm 32 projects (Clean Architecture)
│   ├── Directory.Build.props               # Cấu hình compile tập trung (net9.0, nullable)
│   ├── Directory.Packages.props            # Quản trị phiên bản NuGet tập trung (CPM)
│   ├── global.json                         # Ghim phiên bản .NET 9 SDK
│   ├── src/
│   │   ├── BuildingBlocks/
│   │   │   └── HospitalMS.BuildingBlocks/  # Shared library: Outbox, Audit, Scope Auth, Metrics, Health
│   │   ├── Services/                       # 7 Microservices độc lập (Clean Architecture 4 tầng)
│   │   │   ├── Identity/                   # SSO, JWT, RBAC, App Launcher
│   │   │   ├── Patient/                    # Master Patient Index (MPI), chống trùng, merge hồ sơ
│   │   │   ├── Catalog/                    # ICD-10, dịch vụ kỹ thuật TT39, khoa phòng, cache Redis
│   │   │   ├── Outpatient/                 # Khám ngoại trú, hàng chờ, CLS, kê đơn, viện phí BHYT
│   │   │   ├── Inpatient/                  # Quản lý buồng giường kéo thả, y lệnh, sinh hiệu, xuất viện
│   │   │   ├── Pharmacy/                   # Quản lý kho dược, lô - hạn dùng (FEFO), thẻ kho
│   │   │   └── HealthExam/                 # Khám sức khỏe đoàn, hợp đồng, trạm khám, kết luận TT14
│   │   └── ApiGateway/
│   │       └── HospitalMS.ApiGateway/      # YARP Reverse Proxy định tuyến vi dịch vụ
│   └── tests/
│       ├── HospitalMS.BuildingBlocks.Tests/# Unit tests cho Outbox, Audit, Scopes, Metrics (11 tests)
│       ├── HospitalMS.Identity.Tests/      # Unit tests cho Identity, JWT, Lockout (10 tests)
│       ├── HospitalMS.Patient.Tests/       # Unit tests cho MPI, chống trùng (4 tests)
│       ├── HospitalMS.Catalog.Tests/       # Unit tests cho ICD-10, Catalog cache (2 tests)
│       ├── HospitalMS.Outpatient.Tests/    # Unit tests cho Hàng chờ, Khám, Kê đơn, Viện phí (5 tests)
│       ├── HospitalMS.Inpatient.Tests/     # Unit tests cho Buồng giường, Chuyển giường (5 tests)
│       ├── HospitalMS.Pharmacy.Tests/      # Unit tests cho FEFO, Thẻ kho, Concurrency (7 tests)
│       └── HospitalMS.HealthExam.Tests/    # Unit tests cho Trạm khám, Phân loại TT14 (16 tests)
│
├── frontend/                               # Ứng dụng Portal/Shell Vue 3 + TypeScript
│   ├── package.json
│   ├── vite.config.ts                      # Cấu hình Vite & Proxy sang ApiGateway (port 5100)
│   ├── tailwind.config.js                  # TailwindCSS hệ thống màu bệnh viện (hospital-*)
│   ├── Dockerfile                          # Multi-stage Dockerfile (Node 22 build -> Nginx alpine)
│   └── src/
│       ├── core/components/                # DataTable (phân trang, lọc, sắp xếp), BaseModal, StatusBadge
│       ├── layouts/                        # AppLayout, AuthLayout
│       ├── stores/                         # Pinia stores: auth, appLauncher
│       ├── services/                       # Axios API client, auto-refresh token interceptor
│       ├── i18n/                           # vue-i18n (Tiếng Việt vi / Tiếng Anh en)
│       └── modules/                        # Lazy-loaded modules theo từng phân hệ
│           ├── healthexam/                 # Khám đoàn, gói khám, quy trình trạm khám, TT14
│           ├── outpatient/                 # Màn hình bác sĩ khám, chỉ định CLS, kê đơn
│           ├── inpatient/                  # Sơ đồ buồng giường trực quan kéo thả, y lệnh
│           ├── pharmacy/                   # Quản lý thuốc, xuất nhập tồn, cấp phát đơn thuốc
│           ├── patient/                    # Hồ sơ bệnh nhân MPI
│           ├── catalog/                    # Danh mục ICD-10, dịch vụ kỹ thuật
│           └── admin/                      # Quản trị hệ thống, users, roles, permissions
│
├── deploy/                                 # Cấu hình Nginx và Database Init Script
│   ├── docker/init-databases.sql           # Tự động tạo 7 cơ sở dữ liệu PostgreSQL
│   └── nginx/frontend.conf                 # Cấu hình SPA Nginx reverse proxy & gzip
│
├── docs/                                   # Tài liệu kiến trúc & đặc tả kỹ thuật
│   ├── architecture-overview.md            # Sơ đồ C4, quy trình tích hợp, patterns
│   ├── api-reference.md                    # Đặc tả toàn bộ API các vi dịch vụ
│   ├── deployment-guide.md                 # Hướng dẫn chi tiết triển khai Local & Production
│   └── postman/
│       └── HospitalMS_Complete.postman_collection.json # Bộ Postman collection kiểm thử đầy đủ
│
├── docker-compose.yml                      # Compose môi trường Development
├── docker-compose.prod.yml                 # Compose môi trường Production tối ưu hóa
├── .env.example                            # Biến môi trường mẫu
└── README.md
```

---

## 2. Bảng Phân Bổ Cổng Mạng (Port Mapping)

| Phân hệ / Dịch vụ | Loại | Cổng Host | Cổng Container | Database tương ứng |
| :--- | :--- | :--- | :--- | :--- |
| **Frontend Portal** | Web SPA | `80` (Prod) / `5173` (Dev) | `80` | - |
| **API Gateway (YARP)** | Reverse Proxy | `5100` | `8080` | - |
| **Identity Service** | Microservice | `5101` | `8080` | `hospitalms_identity` |
| **Patient Service (MPI)** | Microservice | `5102` | `8080` | `hospitalms_patient` |
| **Catalog Service** | Microservice | `5103` | `8080` | `hospitalms_catalog` |
| **Outpatient Service** | Microservice | `5104` | `8080` | `hospitalms_outpatient` |
| **Inpatient Service** | Microservice | `5105` | `8080` | `hospitalms_inpatient` |
| **Pharmacy Service** | Microservice | `5106` | `8080` | `hospitalms_pharmacy` |
| **Health Exam Service** | Microservice | `5107` | `8080` | `hospitalms_healthexam` |
| **PostgreSQL** | Database Server | `5432` | `5432` | 7 DBs độc lập |
| **RabbitMQ** | Message Broker | `5672` / `15672` | `5672` / `15672` | - |
| **Redis** | Distributed Cache | `6379` | `6379` | - |
| **Seq** | Log Server | `5341` / `8081` | `5341` / `80` | - |
| **pgAdmin** | DB Management UI | `5050` | `80` | - |

---

## 3. Các Tính Năng Nổi Bật Đã Hoàn Thiện

1. **Transactional Outbox Pattern & Event-Driven:**
   - Bảo đảm tính nhất quán phân tán (Eventual Consistency) qua `OutboxMessage` và `OutboxProcessorBackgroundService`.
   - Cấu hình MassTransit RabbitMQ với chính sách Exponential Retry (3 lần) và tự động cách ly poison message vào Dead-Letter Queue (`{queue}_error`).
2. **Audit Logging Toàn Hệ Thống & Phân Quyền Phạm Vi (Scope Auth):**
   - MediatR `AuditBehavior` tự động ghi vết IP, TraceId, Claims, User, Payload và lưu vết có cấu trúc.
   - `DepartmentScopeHandler` và `WarehouseScopeHandler` kiểm soát truy cập phân tầng theo từng Khoa phòng và Kho thuốc.
3. **Quản Lý Dược Chuẩn FEFO & Optimistic Concurrency:**
   - Tự động trừ tồn theo hạn dùng gần nhất, ngăn chặn race condition khi nhiều quầy phát thuốc cùng lúc.
   - Sổ thẻ kho (`StockLedger`) lưu lịch sử biến động bất biến phục vụ thanh kiểm tra.
4. **Sơ Đồ Giường Bệnh Trực Quan (HTML5 Drag & Drop):**
   - Quản lý trạng thái 4 màu: Trống (Xanh), Có người (Đỏ), Dọn dẹp (Vàng), Bảo trì (Xám).
   - Kéo thả bệnh nhân giữa các giường bệnh để chuyển khoa phòng tự động.
5. **Khám Sức Khỏe Đoàn Đa Chuyên Khoa:**
   - Import danh sách nhân viên từ file Excel có báo lỗi chi tiết từng dòng.
   - Trạm khám động, nhập kết quả CLS và tự động phân loại sức khỏe theo Thông tư 14/2013/TT-BYT.
6. **Observability & Health Checks:**
   - Kiểm tra trạng thái `/health/live` và `/health/ready` (DB, Redis, RabbitMQ ping).
   - Đăng ký chỉ số hiệu năng y tế (`HospitalMetrics`) và OpenTelemetry tracing (`HospitalActivitySource`).

---

## 4. Hướng Dẫn Vận Hành & Khởi Chạy

### 4.1 Khởi chạy Production bằng Docker Compose (Khuyên dùng)
```bash
# Thiết lập biến môi trường
cp .env.example .env

# Build và khởi chạy toàn bộ 15 dịch vụ
docker compose -f docker-compose.prod.yml up -d --build

# Xem trạng thái hoạt động
docker compose -f docker-compose.prod.yml ps
```

### 4.2 Chạy Backend cục bộ (.NET 9)
```bash
# Chạy toàn bộ 60 tests (Tất cả đều PASS 100%)
dotnet test backend/HospitalMS.sln

# Chạy API Gateway
dotnet run --project backend/src/ApiGateway/HospitalMS.ApiGateway/HospitalMS.ApiGateway.csproj
```

### 4.3 Chạy Frontend cục bộ (Vue 3 + Vite)
```bash
cd frontend
npm install
npm run dev
# Truy cập giao diện tại http://localhost:5173
```

---

## 5. Tài Khoản Đăng Nhập Mặc Định

| Vai trò | Tên đăng nhập | Mật khẩu | Ứng dụng được cấp quyền |
| :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `admin` | `Admin@123` | Toàn quyền tất cả các phân hệ |
| **Bác sĩ khám bệnh** | `doctor` | `Doctor@123` | Khám ngoại trú, Bệnh nhân, Danh mục |
| **Bác sĩ nội trú** | `inpatient_doc`| `Doctor@123` | Quản lý nội trú & buồng giường |
| **Dược sĩ** | `pharmacist` | `Pharm@123` | Kho dược, cấp phát đơn thuốc |
| **Lễ tân tiếp đón** | `receptionist`| `Recep@123` | Tiếp đón bệnh nhân, Khám đoàn |

---

## 6. Liên Kết Tài Liệu Kỹ Thuật

- [Sơ đồ Kiến Trúc Tổng Thể & Design Patterns](docs/architecture-overview.md)
- [Đặc Tả Chi Tiết Toàn Bộ API](docs/api-reference.md)
- [Hướng Dẫn Triển Khai & Vận Hành](docs/deployment-guide.md)
- [Postman Collection](docs/postman/HospitalMS_Complete.postman_collection.json)
