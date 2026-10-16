# Hệ Thống Quản Lý Bệnh Viện Tích Hợp (HospitalMS)

Hệ thống quản lý bệnh viện tích hợp chuyên sâu theo tiêu chuẩn y tế Việt Nam (Thông tư 14/2013/TT-BYT, Thông tư 24/2015/TTLT-BYT-BGTVT, ICD-10, BHYT) được thiết kế theo mô hình Monorepo tách biệt rõ ràng `backend/` và `frontend/`.

---

## 1. Cấu Trúc Dự Án (Monorepo Tách Biệt Frontend & Backend)

```
hospital-management-system/
├── backend/                                # Toàn bộ mã nguồn & cấu hình Backend .NET 9
│   ├── HospitalMS.sln                      # Solution gồm 31 projects
│   ├── Directory.Build.props               # Cấu hình build tập trung (net9.0, nullable)
│   ├── Directory.Packages.props            # Quản trị phiên bản NuGet tập trung (CPM)
│   ├── global.json                         # Ghim phiên bản .NET 9 SDK
│   ├── scripts/
│   │   └── setup-solution.sh               # Script CLI tạo solution & tổ chức solution folders
│   ├── src/
│   │   ├── BuildingBlocks/
│   │   │   └── HospitalMS.BuildingBlocks/  # Thư viện dùng chung (Entity, Result, Audit, Middleware)
│   │   ├── Services/                       # 7 Microservices theo Clean Architecture (4 tầng)
│   │   │   ├── Identity/                   # SSO, RBAC, Users, Roles, Permissions, App Launcher
│   │   │   ├── Patient/                    # Master Patient Index (MPI), CCCD, BHYT
│   │   │   ├── Catalog/                    # ICD-10, Dịch vụ kỹ thuật TT39, Khoa phòng
│   │   │   ├── Outpatient/                 # Khám ngoại trú, CLS, kê đơn, viện phí
│   │   │   ├── Inpatient/                  # Quản lý nội trú, sơ đồ giường, y lệnh
│   │   │   ├── Pharmacy/                   # Quản lý kho dược, lô - hạn dùng (FEFO), cấp phát
│   │   │   └── HealthExam/                 # Khám sức khỏe đoàn, trạm khám đa khoa, TT14/TT24
│   │   └── ApiGateway/
│   │       └── HospitalMS.ApiGateway/      # YARP Reverse Proxy điều hướng vi dịch vụ
│   └── tests/
│       └── HospitalMS.BuildingBlocks.Tests/# Unit tests (xUnit, FluentAssertions, Moq)
│
├── frontend/                               # Ứng dụng Portal/Shell Vue 3 + TypeScript
│   ├── package.json
│   ├── vite.config.ts                      # Cấu hình Vite & Proxy sang ApiGateway (port 5100)
│   ├── tailwind.config.js                  # TailwindCSS hệ thống màu bệnh viện (hospital-*)
│   ├── Dockerfile                          # Multi-stage Dockerfile (Node 22 build -> Nginx alpine)
│   └── src/
│       ├── components/                     # AppHeader, AppLauncherModal, AppSidebar
│       ├── layouts/                        # AppLayout, AuthLayout
│       ├── stores/                         # Pinia stores: auth, appLauncher
│       ├── services/                       # Axios API client, authService
│       ├── i18n/                           # vue-i18n (Tiếng Việt vi / Tiếng Anh en)
│       ├── views/                          # LoginView, DashboardView, NotFoundView
│       └── modules/                        # Lazy-loaded modules theo từng phân hệ
│           ├── healthexam/                 # Khám sức khỏe (Hợp đồng, luồng trạm, TT14)
│           ├── outpatient/                 # Khám ngoại trú
│           ├── inpatient/                  # Nội trú & giường bệnh
│           ├── pharmacy/                   # Kho dược & vật tư
│           ├── patient/                    # Hồ sơ bệnh nhân MPI
│           ├── catalog/                    # Danh mục
│           └── admin/                      # Quản trị hệ thống
│
├── deploy/                                 # Cấu hình triển khai Docker & Database
│   ├── docker-compose.yml
│   └── docker/
│       └── init-databases.sql              # Script tự động tạo 7 database PostgreSQL
│
├── docs/                                   # Tài liệu kiến trúc & thiết kế hệ thống
│   └── architecture.md
├── docker-compose.yml                      # Điều phối toàn bộ hệ thống
├── .env.example / .env                     # Biến môi trường mẫu cho hạ tầng
└── .editorconfig                           # Quy chuẩn coding style C# & Web
```

---

## 2. Bảng Danh Mục Cổng (Port Mapping)

| Thành phần | Loại | Cổng Host | Cổng Container | Mô tả |
|------------|------|-----------|----------------|-------|
| **frontend** | Web UI | `3000` | `80` | Portal Shell Vue 3 + App Launcher |
| **api-gateway** | Gateway | `5100` | `8080` | YARP Reverse Proxy |
| **identity-api** | Backend Service | `5101` | `8080` | Quản trị định danh & phân quyền |
| **patient-api** | Backend Service | `5102` | `8080` | Master Patient Index (MPI) |
| **catalog-api** | Backend Service | `5103` | `8080` | Danh mục dùng chung (ICD-10, TT39) |
| **outpatient-api**| Backend Service | `5104` | `8080` | Khám ngoại trú & viện phí |
| **inpatient-api** | Backend Service | `5105` | `8080` | Nội trú & sơ đồ giường bệnh |
| **pharmacy-api** | Backend Service | `5106` | `8080` | Kho dược & Lô hạn dùng |
| **healthexam-api**| Backend Service | `5107` | `8080` | Khám sức khỏe (Trọng tâm) |
| **postgres** | Infrastructure | `5432` | `5432` | 7 PostgreSQL databases |
| **rabbitmq** | Message Broker | `5672`, `15672`| `5672`, `15672` | RabbitMQ AMQP & Management Dashboard |
| **redis** | Caching | `6379` | `6379` | Redis Cache |
| **seq** | Log Server | `5341`, `8081` | `5341`, `80` | Seq Ingestion & Web UI |
| **pgadmin** | DB Admin | `5050` | `80` | Web UI quản trị PostgreSQL |

---

## 3. Hướng Dẫn Vận Hành

### Cách 1: Khởi động toàn bộ qua Docker Compose (Khuyên dùng)
```bash
# Khởi chạy toàn bộ 15 container (Hạ tầng + 8 backend services + Frontend)
docker-compose up -d --build

# Xem trạng thái các container
docker-compose ps

# Xem log thời gian thực của toàn bộ hệ thống
docker-compose logs -f
```

### Cách 2: Chạy Backend cục bộ (.NET 9)
```bash
cd backend
# Build toàn bộ 31 projects
dotnet build HospitalMS.sln

# Chạy Unit Tests
dotnet test

# Chạy một service độc lập (ví dụ ApiGateway)
dotnet run --project src/ApiGateway/HospitalMS.ApiGateway/HospitalMS.ApiGateway.csproj
```

### Cách 3: Chạy Frontend cục bộ (Vue 3 Dev Server)
```bash
cd frontend
# Cài đặt thư viện
npm install

# Khởi chạy server phát triển (Hot-reload tại http://localhost:3000)
npm run dev

# Kiểm tra bundle production
npm run build
```

---

## 4. Kiểm Tra & Đánh Giá Kết Quả (Health Check Verification)

Sau khi khởi chạy hệ thống, kiểm tra các endpoint `/health` của các dịch vụ qua Gateway hoặc trực tiếp:
- **API Gateway Health**: `curl http://localhost:5100/health` -> Trả về `Healthy`
- **Identity API**: `curl http://localhost:5101/health` -> Trả về `Healthy`
- **Patient API**: `curl http://localhost:5102/health` -> Trả về `Healthy`
- **Catalog API**: `curl http://localhost:5103/health` -> Trả về `Healthy`
- **Outpatient API**: `curl http://localhost:5104/health` -> Trả về `Healthy`
- **Inpatient API**: `curl http://localhost:5105/health` -> Trả về `Healthy`
- **Pharmacy API**: `curl http://localhost:5106/health` -> Trả về `Healthy`
- **HealthExam API**: `curl http://localhost:5107/health` -> Trả về `Healthy`
- **Định tuyến qua YARP Gateway**:
  - `curl http://localhost:5100/identity/health`
  - `curl http://localhost:5100/healthexam/health`
- **Seq Log Dashboard**: Truy cập `http://localhost:8081`
- **RabbitMQ Management**: Truy cập `http://localhost:15672` (User/Pass: `guest`/`guest`)
- **Frontend Portal**: Truy cập `http://localhost:3000` (Đăng nhập tài khoản `admin` / `admin123`)
