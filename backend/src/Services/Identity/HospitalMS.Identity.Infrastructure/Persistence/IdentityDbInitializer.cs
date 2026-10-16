using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Domain.Entities;
using HospitalMS.Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppEntity = HospitalMS.Identity.Domain.Entities.Application;

namespace HospitalMS.Identity.Infrastructure.Persistence;

public class IdentityDbInitializer(
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    ILogger<IdentityDbInitializer> logger)
{
    public async Task InitializeAsync()
    {
        try
        {
            if (dbContext.Database.IsRelational())
            {
                await dbContext.Database.MigrateAsync();
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            await SeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the Identity database.");
            throw;
        }
    }

    private async Task SeedAsync()
    {
        // 1. Seed Departments
        if (!await dbContext.Departments.AnyAsync())
        {
            var departments = new List<Department>
            {
                new() { Code = "BGD", Name = "Ban Giám Đốc", PhoneNumber = "024.3825.0001" },
                new() { Code = "KKB", Name = "Khoa Khám Bệnh", PhoneNumber = "024.3825.0002" },
                new() { Code = "KNOI", Name = "Khoa Nội Tổng Hợp", PhoneNumber = "024.3825.0003" },
                new() { Code = "KNGOAI", Name = "Khoa Ngoại Tổng Quát", PhoneNumber = "024.3825.0004" },
                new() { Code = "KD", Name = "Khoa Dược", PhoneNumber = "024.3825.0005" },
                new() { Code = "XN", Name = "Khoa Xét Nghiệm & CĐHA", PhoneNumber = "024.3825.0006" }
            };
            dbContext.Departments.AddRange(departments);
            await dbContext.SaveChangesAsync();
        }

        // 2. Seed Applications
        if (!await dbContext.Applications.AnyAsync())
        {
            var apps = new List<AppEntity>
            {
                new()
                {
                    Code = "HEALTH_EXAM",
                    Name = "Khám sức khỏe",
                    Description = "Quản lý hợp đồng đoàn, gói khám, quy trình trạm đa chuyên khoa, kết luận TT14/TT24",
                    Icon = "HeartPulse",
                    Color = "from-emerald-500 to-teal-600",
                    Path = "/healthexam",
                    DisplayOrder = 1
                },
                new()
                {
                    Code = "OUTPATIENT",
                    Name = "Khám ngoại trú",
                    Description = "Tiếp đón, buồng khám bác sĩ, chỉ định cận lâm sàng, kê đơn điện tử, viện phí",
                    Icon = "Stethoscope",
                    Color = "from-blue-500 to-indigo-600",
                    Path = "/outpatient",
                    DisplayOrder = 2
                },
                new()
                {
                    Code = "INPATIENT",
                    Name = "Quản lý nội trú",
                    Description = "Nhập viện, sơ đồ giường bệnh trực quan, chuyển khoa, y lệnh điều trị, hồ sơ bệnh án",
                    Icon = "BedDouble",
                    Color = "from-violet-500 to-purple-600",
                    Path = "/inpatient",
                    DisplayOrder = 3
                },
                new()
                {
                    Code = "PHARMACY",
                    Name = "Kho dược & Vật tư",
                    Description = "Quản lý nhập xuất tồn, lô hạn dùng (FEFO/FIFO), duyệt đơn, cấp phát thuốc",
                    Icon = "Pill",
                    Color = "from-amber-500 to-orange-600",
                    Path = "/pharmacy",
                    DisplayOrder = 4
                },
                new()
                {
                    Code = "PATIENT",
                    Name = "Hồ sơ Bệnh nhân (MPI)",
                    Description = "Mã bệnh nhân duy nhất, CCCD, thẻ BHYT và lịch sử khám chữa bệnh toàn viện",
                    Icon = "Users",
                    Color = "from-cyan-500 to-blue-600",
                    Path = "/patient",
                    DisplayOrder = 5
                },
                new()
                {
                    Code = "CATALOG",
                    Name = "Danh mục dùng chung",
                    Description = "ICD-10, dịch vụ kỹ thuật TT39, khoa phòng, phân loại thuốc và vật tư y tế",
                    Icon = "Database",
                    Color = "from-slate-600 to-slate-800",
                    Path = "/catalog",
                    DisplayOrder = 6
                },
                new()
                {
                    Code = "ADMIN",
                    Name = "Quản trị hệ thống",
                    Description = "Tài khoản người dùng, vai trò (Roles), ma trận quyền RBAC, giám sát nhật ký",
                    Icon = "ShieldCheck",
                    Color = "from-rose-500 to-red-600",
                    Path = "/admin",
                    DisplayOrder = 7
                }
            };
            dbContext.Applications.AddRange(apps);
            await dbContext.SaveChangesAsync();
        }

        // 3. Seed Granular Permissions
        if (!await dbContext.Permissions.AnyAsync())
        {
            var permissions = new List<Permission>
            {
                // Identity / System Permissions
                new() { Code = "users.view", Name = "Xem danh sách người dùng", Module = "ADMIN" },
                new() { Code = "users.create", Name = "Thêm mới người dùng", Module = "ADMIN" },
                new() { Code = "users.update", Name = "Cập nhật người dùng", Module = "ADMIN" },
                new() { Code = "users.delete", Name = "Xóa người dùng", Module = "ADMIN" },
                new() { Code = "roles.view", Name = "Xem danh sách vai trò", Module = "ADMIN" },
                new() { Code = "roles.create", Name = "Thêm mới vai trò", Module = "ADMIN" },
                new() { Code = "roles.update", Name = "Cập nhật vai trò", Module = "ADMIN" },
                new() { Code = "roles.delete", Name = "Xóa vai trò", Module = "ADMIN" },

                // HealthExam Permissions (Trọng tâm)
                new() { Code = "healthexam.view", Name = "Xem phân hệ khám sức khỏe", Module = "HEALTH_EXAM" },
                new() { Code = "healthexam.contract.create", Name = "Tạo mới hợp đồng khám đoàn", Module = "HEALTH_EXAM" },
                new() { Code = "healthexam.contract.update", Name = "Cập nhật hợp đồng khám đoàn", Module = "HEALTH_EXAM" },
                new() { Code = "healthexam.excel.import", Name = "Import danh sách đoàn từ Excel", Module = "HEALTH_EXAM" },
                new() { Code = "healthexam.reception.checkin", Name = "Check-in & Cấp số thứ tự KSK", Module = "HEALTH_EXAM" },
                new() { Code = "healthexam.station.examine", Name = "Khám & Nhập kết quả chuyên khoa", Module = "HEALTH_EXAM" },
                new() { Code = "healthexam.lab.enter", Name = "Nhập kết quả Xét nghiệm & CĐHA", Module = "HEALTH_EXAM" },
                new() { Code = "healthexam.conclusion.approve", Name = "Kết luận phân loại sức khỏe TT14/TT24", Module = "HEALTH_EXAM" },
                new() { Code = "healthexam.report.export", Name = "Xuất sổ KSK & Báo cáo tổng hợp đoàn", Module = "HEALTH_EXAM" },

                // Outpatient Permissions
                new() { Code = "outpatient.view", Name = "Xem phân hệ ngoại trú", Module = "OUTPATIENT" },
                new() { Code = "outpatient.reception.register", Name = "Tiếp đón bệnh nhân", Module = "OUTPATIENT" },
                new() { Code = "outpatient.exam.diagnose", Name = "Khám bệnh & Chẩn đoán ICD-10", Module = "OUTPATIENT" },
                new() { Code = "outpatient.prescription.create", Name = "Kê đơn thuốc ngoại trú", Module = "OUTPATIENT" },
                new() { Code = "outpatient.billing.collect", Name = "Thu viện phí & Đồng chi trả BHYT", Module = "OUTPATIENT" },

                // Inpatient Permissions
                new() { Code = "inpatient.view", Name = "Xem phân hệ nội trú", Module = "INPATIENT" },
                new() { Code = "inpatient.admission.create", Name = "Tiếp nhận bệnh nhân nhập viện", Module = "INPATIENT" },
                new() { Code = "inpatient.bed.assign", Name = "Quản lý & xếp giường bệnh", Module = "INPATIENT" },
                new() { Code = "inpatient.order.create", Name = "Lên y lệnh thuốc & dịch truyền", Module = "INPATIENT" },
                new() { Code = "inpatient.discharge.process", Name = "Tổng kết bệnh án & ra viện", Module = "INPATIENT" },

                // Pharmacy Permissions
                new() { Code = "pharmacy.view", Name = "Xem phân hệ kho dược", Module = "PHARMACY" },
                new() { Code = "pharmacy.import.create", Name = "Lập phiếu nhập kho", Module = "PHARMACY" },
                new() { Code = "pharmacy.export.create", Name = "Lập phiếu xuất kho", Module = "PHARMACY" },
                new() { Code = "pharmacy.inventory.check", Name = "Kiểm kê kho & cảnh báo FEFO", Module = "PHARMACY" },
                new() { Code = "pharmacy.dispense.approve", Name = "Duyệt & cấp phát thuốc theo đơn", Module = "PHARMACY" },

                // Patient MPI Permissions
                new() { Code = "patient.view", Name = "Xem hồ sơ bệnh nhân MPI", Module = "PATIENT" },
                new() { Code = "patient.create", Name = "Tạo mới định danh bệnh nhân", Module = "PATIENT" },
                new() { Code = "patient.update", Name = "Cập nhật thông tin CCCD / BHYT", Module = "PATIENT" },

                // Catalog Permissions
                new() { Code = "catalog.view", Name = "Xem danh mục dùng chung", Module = "CATALOG" },
                new() { Code = "catalog.manage", Name = "Cập nhật danh mục ICD-10 & Dịch vụ kỹ thuật", Module = "CATALOG" }
            };
            dbContext.Permissions.AddRange(permissions);
            await dbContext.SaveChangesAsync();
        }

        // 4. Seed Sample Roles
        if (!await dbContext.Roles.AnyAsync())
        {
            var allPermissions = await dbContext.Permissions.ToListAsync();
            var allApps = await dbContext.Applications.ToListAsync();

            // Admin Role
            var adminRole = new Role
            {
                Code = "Admin",
                Name = "Quản Trị Viên Hệ Thống",
                Description = "Toàn quyền quản trị và điều phối mọi phân hệ",
                IsSystemRole = true
            };
            foreach (var perm in allPermissions) adminRole.RolePermissions.Add(new RolePermission { Role = adminRole, Permission = perm });
            foreach (var app in allApps) adminRole.RoleApplications.Add(new RoleApplication { Role = adminRole, Application = app });

            // Doctor (Bác sĩ) Role
            var doctorRole = new Role
            {
                Code = "Doctor",
                Name = "Bác Sĩ Điều Trị",
                Description = "Khám chữa bệnh ngoại trú, y lệnh nội trú và kết luận KSK",
                IsSystemRole = false
            };
            var doctorPerms = allPermissions.Where(p =>
                p.Code.StartsWith("healthexam.") ||
                p.Code.StartsWith("outpatient.exam.") ||
                p.Code.StartsWith("outpatient.prescription.") ||
                p.Code.StartsWith("inpatient.") ||
                p.Code == "patient.view" ||
                p.Code == "catalog.view" ||
                p.Code == "outpatient.view"
            ).ToList();
            foreach (var p in doctorPerms) doctorRole.RolePermissions.Add(new RolePermission { Role = doctorRole, Permission = p });
            var doctorApps = allApps.Where(a => new[] { "HEALTH_EXAM", "OUTPATIENT", "INPATIENT", "PATIENT", "CATALOG" }.Contains(a.Code)).ToList();
            foreach (var a in doctorApps) doctorRole.RoleApplications.Add(new RoleApplication { Role = doctorRole, Application = a });

            // Nurse (Điều dưỡng) Role
            var nurseRole = new Role
            {
                Code = "Nurse",
                Name = "Điều Dưỡng Viên",
                Description = "Thực hiện trạm khám thể lực KSK, chăm sóc và giường bệnh nội trú",
                IsSystemRole = false
            };
            var nursePerms = allPermissions.Where(p =>
                p.Code == "healthexam.view" ||
                p.Code == "healthexam.station.examine" ||
                p.Code == "inpatient.view" ||
                p.Code == "inpatient.bed.assign" ||
                p.Code == "patient.view"
            ).ToList();
            foreach (var p in nursePerms) nurseRole.RolePermissions.Add(new RolePermission { Role = nurseRole, Permission = p });
            var nurseApps = allApps.Where(a => new[] { "HEALTH_EXAM", "INPATIENT", "PATIENT" }.Contains(a.Code)).ToList();
            foreach (var a in nurseApps) nurseRole.RoleApplications.Add(new RoleApplication { Role = nurseRole, Application = a });

            // Receptionist (Lễ tân) Role
            var receptionistRole = new Role
            {
                Code = "Receptionist",
                Name = "Lễ Tân & Tiếp Đón",
                Description = "Tiếp đón bệnh nhân ngoại trú, check-in đoàn KSK và thu viện phí",
                IsSystemRole = false
            };
            var receptionistPerms = allPermissions.Where(p =>
                p.Code == "patient.view" ||
                p.Code == "patient.create" ||
                p.Code == "patient.update" ||
                p.Code == "outpatient.view" ||
                p.Code == "outpatient.reception.register" ||
                p.Code == "outpatient.billing.collect" ||
                p.Code == "healthexam.view" ||
                p.Code == "healthexam.reception.checkin"
            ).ToList();
            foreach (var p in receptionistPerms) receptionistRole.RolePermissions.Add(new RolePermission { Role = receptionistRole, Permission = p });
            var recApps = allApps.Where(a => new[] { "PATIENT", "OUTPATIENT", "HEALTH_EXAM" }.Contains(a.Code)).ToList();
            foreach (var a in recApps) receptionistRole.RoleApplications.Add(new RoleApplication { Role = receptionistRole, Application = a });

            // Pharmacist (Dược sĩ) Role
            var pharmacistRole = new Role
            {
                Code = "Pharmacist",
                Name = "Dược Sĩ Lâm Sàng & Cấp Phát",
                Description = "Duyệt đơn thuốc ngoại/nội trú và cấp phát thuốc",
                IsSystemRole = false
            };
            var pharmacistPerms = allPermissions.Where(p =>
                p.Code.StartsWith("pharmacy.") ||
                p.Code == "catalog.view" ||
                p.Code == "patient.view"
            ).ToList();
            foreach (var p in pharmacistPerms) pharmacistRole.RolePermissions.Add(new RolePermission { Role = pharmacistRole, Permission = p });
            var pharmApps = allApps.Where(a => new[] { "PHARMACY", "CATALOG", "PATIENT" }.Contains(a.Code)).ToList();
            foreach (var a in pharmApps) pharmacistRole.RoleApplications.Add(new RoleApplication { Role = pharmacistRole, Application = a });

            // Storekeeper (Thủ kho dược) Role
            var storekeeperRole = new Role
            {
                Code = "Storekeeper",
                Name = "Thủ Kho Dược & Vật Tư",
                Description = "Quản lý nhập kho, xuất kho nội bộ và kiểm kê lô hạn dùng FEFO",
                IsSystemRole = false
            };
            var storekeeperPerms = allPermissions.Where(p =>
                p.Code == "pharmacy.view" ||
                p.Code == "pharmacy.import.create" ||
                p.Code == "pharmacy.export.create" ||
                p.Code == "pharmacy.inventory.check"
            ).ToList();
            foreach (var p in storekeeperPerms) storekeeperRole.RolePermissions.Add(new RolePermission { Role = storekeeperRole, Permission = p });
            var storeApps = allApps.Where(a => a.Code == "PHARMACY").ToList();
            foreach (var a in storeApps) storekeeperRole.RoleApplications.Add(new RoleApplication { Role = storekeeperRole, Application = a });

            dbContext.Roles.AddRange(adminRole, doctorRole, nurseRole, receptionistRole, pharmacistRole, storekeeperRole);
            await dbContext.SaveChangesAsync();
        }

        // 5. Seed Users
        if (!await dbContext.Users.AnyAsync())
        {
            var adminRole = await dbContext.Roles.FirstAsync(r => r.Code == "Admin");
            var doctorRole = await dbContext.Roles.FirstAsync(r => r.Code == "Doctor");
            var nurseRole = await dbContext.Roles.FirstAsync(r => r.Code == "Nurse");
            var recRole = await dbContext.Roles.FirstAsync(r => r.Code == "Receptionist");
            var pharmRole = await dbContext.Roles.FirstAsync(r => r.Code == "Pharmacist");
            var storeRole = await dbContext.Roles.FirstAsync(r => r.Code == "Storekeeper");

            var bgdDept = await dbContext.Departments.FirstOrDefaultAsync(d => d.Code == "BGD");
            var kkbDept = await dbContext.Departments.FirstOrDefaultAsync(d => d.Code == "KKB");
            var knoiDept = await dbContext.Departments.FirstOrDefaultAsync(d => d.Code == "KNOI");
            var kdDept = await dbContext.Departments.FirstOrDefaultAsync(d => d.Code == "KD");

            var users = new List<User>
            {
                new()
                {
                    Username = "admin",
                    FullName = "Quản Trị Viên Hệ Thống",
                    Email = "admin@hospital.local",
                    PhoneNumber = "0901.000.001",
                    PasswordHash = passwordHasher.HashPassword("Admin@123"),
                    DepartmentId = bgdDept?.Id,
                    Status = UserStatus.Active,
                    UserRoles = [new UserRole { Role = adminRole }]
                },
                new()
                {
                    Username = "doctor",
                    FullName = "BSCKI. Nguyễn Thành Nam",
                    Email = "doctor@hospital.local",
                    PhoneNumber = "0901.000.002",
                    PasswordHash = passwordHasher.HashPassword("Doctor@123"),
                    DepartmentId = knoiDept?.Id,
                    Status = UserStatus.Active,
                    UserRoles = [new UserRole { Role = doctorRole }]
                },
                new()
                {
                    Username = "nurse",
                    FullName = "ĐD. Lê Thị Mai",
                    Email = "nurse@hospital.local",
                    PhoneNumber = "0901.000.003",
                    PasswordHash = passwordHasher.HashPassword("Nurse@123"),
                    DepartmentId = knoiDept?.Id,
                    Status = UserStatus.Active,
                    UserRoles = [new UserRole { Role = nurseRole }]
                },
                new()
                {
                    Username = "receptionist",
                    FullName = "CN. Trần Thị Thu Thủy",
                    Email = "reception@hospital.local",
                    PhoneNumber = "0901.000.004",
                    PasswordHash = passwordHasher.HashPassword("Reception@123"),
                    DepartmentId = kkbDept?.Id,
                    Status = UserStatus.Active,
                    UserRoles = [new UserRole { Role = recRole }]
                },
                new()
                {
                    Username = "pharmacist",
                    FullName = "DS. Hoàng Minh Đức",
                    Email = "pharmacy@hospital.local",
                    PhoneNumber = "0901.000.005",
                    PasswordHash = passwordHasher.HashPassword("Pharmacy@123"),
                    DepartmentId = kdDept?.Id,
                    Status = UserStatus.Active,
                    UserRoles = [new UserRole { Role = pharmRole }]
                },
                new()
                {
                    Username = "storekeeper",
                    FullName = "Thủ kho. Vũ Văn Hùng",
                    Email = "storage@hospital.local",
                    PhoneNumber = "0901.000.006",
                    PasswordHash = passwordHasher.HashPassword("Storage@123"),
                    DepartmentId = kdDept?.Id,
                    Status = UserStatus.Active,
                    UserRoles = [new UserRole { Role = storeRole }]
                }
            };

            dbContext.Users.AddRange(users);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Database seeded successfully with initial admin, roles, and sample hospital staff.");
        }
    }
}

