##📋 AttendanceMaSys - Hệ Thống Quản Lý Chấm Công & Nhân Sự

  🖼 Image: .NET 10 → https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet
  
  🖼 Image: Architecture → https://img.shields.io/badge/Architecture-Clean%20Architecture-brightgreen
  
  🖼 Image: Database → https://img.shields.io/badge/Database-SQL%20Server-red?logo=microsoftsqlserver
  
  🖼 Image: License → https://img.shields.io/badge/License-MIT-blue

  AttendanceMaSys là hệ thống quản lý nhân sự và chấm công tự động được xây dựng trên nền tảng .NET 10 theo kiến trúc Clean
  Architecture và mô hình CQRS (MediatR). Hệ thống cung cấp đầy đủ các tính năng quản lý nhân sự đa cấp bậc, chấm công linh
  hoạt, xử lý nhập liệu Excel thông minh và phân quyền bảo mật nghiêm ngặt.
  ──────
  ## 🌟 Tính Năng Nổi Bật

  ### 1. 👥 Quản Lý Nhân Sự & Đa Dạng Cấu Trúc Chức Danh

  - Hỗ trợ quản lý đa dạng đối tượng nhân sự với các thuộc tính đặc thụ:
      - Manager / Giám Đốc / Trưởng Phòng: Quản lý cấp bậc (GeneralManager, DepartmentManager).
      - Developer: Quản lý theo Band cấp độ và Hướng kỹ thuật chuyên môn.
      - QA: Quản lý theo Band cấp độ và Flag kỹ năng Automation / Coding.
      - Nhân viên thông thường / Thực tập sinh (Intern).


  ### 2. ⏱️ Chấm Công & Quản Lý Lịch Sử

  - Check-in / Check-out: Chấm công thời gian thực cho nhân viên đang hoạt động.
  - Tra cứu lịch sử chấm công:
      - Cá nhân tự xem lịch sử chấm công của mình.
      - Trưởng phòng xem lịch sử chấm công theo Phòng ban (IT, HR, Sales, ...).
      - Ban Giám đốc / Admin xem toàn bộ lịch sử chấm công hệ thống.


  ### 3. 📊 Nhập Hàng Loạt Từ File Excel (.xlsx)

  - Đa ngôn ngữ Header: Tự động nhận diện tiêu đề cột bằng Tiếng Việt hoặc Tiếng Anh (hỗ trợ cả tiêu đề gộp như Email / Tài
  khoản).
  - Chế độ Chạy thử (Preview / Dry-run): Đọc, kiểm tra tính hợp lệ dữ liệu, phát hiện dòng lỗi (trùng Email, sai định dạng,
  thiếu thông tin) trước khi quyết định lưu vào CSDL.
  - Xử lý Bất đồng bộ (Async Import): Khởi chạy tiến trình chạy ngầm cho các tập tin Excel kích thước lớn (hàng nghìn dòng),
  hỗ trợ API truy vấn trạng thái và xem log lỗi chi tiết (Job Tracking).
  - Tải File Mẫu: API cung cấp file mẫu Employees_Import_Template.xlsx chuẩn.

  ### 4. 🔄 Quản Lý Điều Chuyển Vị Trí & Cho Nghỉ Việc

  - Cập nhật vị trí / Thăng chức: Cho phép thay đổi Phòng ban, Chức vụ, Cấp bậc (Band) và tự động đồng bộ Role truy cập hệ
  thống.
  - Cho nghỉ việc / Sa thải: Đánh dấu nhân viên ngưng hoạt động (IsActive = false), khóa tài khoản đăng nhập vĩnh viễn và
  thu hồi toàn bộ quyền truy cập. Chặn tuyệt đối việc chấm công hoặc đăng nhập lại.

  ### 5. 🔒 Bảo Mật & Phân Quyền (JWT Authentication)

  - Xác thực bằng JWT Bearer Token.
  - Phân quyền theo vai trò (Role-based Authorization): Admin, GeneralManager, DepartmentManager, Employee.
  ──────
  ## 🛠️ Công Nghệ Sử Dụng

  - Framework: .NET 10.0 (ASP.NET Core Minimal APIs)
  - Kiến trúc: Clean Architecture (Domain, Application, Infrastructure, Web)
  - Pattern: CQRS (MediatR), Repository Pattern
  - Database / ORM: SQL Server / LocalDB, Entity Framework Core 10 (Identity), ADO.NET (Microsoft.Data.SqlClient)
  - Excel Processing: MiniExcel (Tối ưu hiệu năng và bộ nhớ)
  - Validation: FluentValidation
  - API Documentation: OpenAPI / Scalar API Reference
  - Console Client: C# Interactive CLI Application
  ──────
  ## 📁 Cấu Trúc Dự Án

    AttendanceMaSys/
    ├── src/
    │   ├── Domain/                 # Entity, Enum, Exceptions, Constants
    │   ├── Application/            # DTOs, CQRS Commands/Queries, Interfaces, Validators
    │   ├── Infrastructure/         # EF Core, Identity Service, Repositories (ADO.NET), Services
    │   ├── Web/                    # Minimal API Endpoints, Middlewares, OpenAPI Configuration
    │   └── ConsoleClient/          # Giao diện dòng lệnh CLI giao tiếp REST API
    ├── tests/
    │   ├── Application.UnitTests/  # Unit test cho logic nghiệp vụ & Import Excel
    │   └── Domain.UnitTests/       # Unit test cho Domain Model
    ├── FileExcel/                  # Thư mục chứa các file Excel mẫu & test
    └── AttendanceMaSys.slnx        # Solution file (.NET 10)
  ──────
  ## 📋 Quy Định Cột Trong File Excel Import

  File nhập liệu đầu vào bắt buộc là định dạng .xlsx. Hệ thống hỗ trợ đọc tiêu đề cột bằng Tiếng Việt hoặc Tiếng Anh:

    | Tên cột Tiếng Việt | Tên cột Tiếng Anh | Loại dữ liệu | Bắt buộc | Mô tả / Giá trị mẫu |
    |---|---|---|---|---|
    | Họ và tên | Full Name | Chuỗi | Có | Nguyễn Văn A (Hệ thống tự tách Họ & Tên) |
    | Email / Tài khoản | Email | Chuỗi | Có | nguyenvana@company.com (Duy nhất) |
    | Số điện thoại | Phone Number | Chuỗi | Không | 0901234567 |
    | Giới tính | Gender | Chuỗi | Không | Nam / Nữ hoặc Male / Female |
    | Phòng ban | Department | Chuỗi | Có | IT, HR, Sales, Marketing, Finance |
    | Loại nhân viên | Employee Type | Chuỗi | Có | Developer, QA, Manager, Employee |
    | Cấp độ (Band) | Band | Số | Không | 1, 2, 3, 4 (Dành cho Dev / QA) |
    | Hướng kỹ thuật | Technical Direction | Chuỗi | Không | .NET / Backend, React / Frontend |
    | Kỹ năng Code | Coding Skills | Logic | Không | Có / Không hoặc True / False (QA) |
    | Loại quản lý | Manager Type | Chuỗi | Không | GeneralManager, DepartmentManager |
  ──────
  ## 🚀 Hướng Dẫn Cài Đặt & Khởi Chạy

  ### 1. Yêu Cầu Tiền Đề

  - Cài đặt .NET 10 SDK (phiên bản 10.0.200 trở lên).
  - Cài đặt Microsoft SQL Server hoặc LocalDB.

  ### 2. Cấu Hình Cơ Sở Dữ Liệu

  Mở file src/Web/appsettings.json và cập nhật chuỗi kết nối Database phù hợp với máy của bạn:

    "ConnectionStrings": {
      "Database": "Server=(localdb)\\mssqllocaldb;Database=AttendanceMaSysDb;Trusted_Connection=True;
  MultipleActiveResultSets=true"
    }

  ### 3. Biên Dịch & Chạy Server Web API

  Mở cửa sổ dòng lệnh tại thư mục gốc dự án:

    # Phục hồi packages và build dự án
    dotnet build AttendanceMaSys.slnx

    # Khởi chạy server Web API
    dotnet run --project src/Web/Web.csproj

  │ Lưu ý: Lần đầu khởi chạy, ứng dụng sẽ tự động tạo CSDL, cập nhật bảng và nạp dữ liệu mẫu (Seed Data) bao gồm các tài
  │ khoản hệ thống.

  ### 4. Chạy Ứng Dụng Console Client (Giao Diện CLI)

  Mở một cửa sổ terminal mới và khởi chạy:

    dotnet run --project src/ConsoleClient/ConsoleClient.csproj
  ──────
  ## 🧪 Chạy Kiểm Thử Tự Động (Unit Tests)

  Dự án đi kèm bộ kiểm thử tự động toàn diện kiểm tra logic nghiệp vụ và khả năng đọc file Excel:

    dotnet test --logger "console;verbosity=normal"
  ──────
  ## 📚 Tài Liệu API Reference

  Sau khi khởi chạy ứng dụng Web, bạn có thể truy cập giao diện tương tác API Swagger / Scalar UI tại địa chỉ:

  - Scalar API Reference: https://localhost:5160/scalar/v1
  - OpenAPI Json Document: https://localhost:5160/openapi/v1.json
  ──────
  ## 📄 Trích Lược Các Tài Khoản Mẫu (Seed Data)

    | Email | Mật khẩu | Phân quyền | Vai trò |
    |---|---|---|---|
    | 'admin@company.com' | 'Admin123!' | Admin | Quản trị viên hệ thống |
    | 'gm@company.com' | 'Manager123!' | GeneralManager | Giám đốc điều hành |
    | 'deptmanager.it@company.com' | 'Manager123!' | DepartmentManager | Trưởng phòng IT |
    | 'lead.dev@company.com' | 'Employee123!' | Employee | Tech Lead Developer |
    | 'automation.qa@company.com' | 'Employee123!' | Employee | Senior Automation QA |    
  ──────
  ## 📝 License

  Dự án được phân phối dưới giấy phép MIT License.
