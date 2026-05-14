# BE_SmartCenter — Backend Documentation

> Backend API cho hệ thống học trực tuyến **SmartCenter**, xây dựng trên nền **.NET 8**, kiến trúc **3-layer**, cơ sở dữ liệu **PostgreSQL**.

---

## Mục lục

1. [Tổng quan hệ thống](#1-tổng-quan-hệ-thống)
2. [Kiến trúc dự án](#2-kiến-trúc-dự-án)
3. [Công nghệ sử dụng](#3-công-nghệ-sử-dụng)
4. [Cấu trúc thư mục](#4-cấu-trúc-thư-mục)
5. [Domain & Entity](#5-domain--entity)
6. [Cấu hình môi trường](#6-cấu-hình-môi-trường)
7. [Chạy dự án](#7-chạy-dự-án)
8. [Chạy với Docker](#8-chạy-với-docker)
9. [Database Migration & Seed Data](#9-database-migration--seed-data)
10. [Authentication & Authorization](#10-authentication--authorization)
11. [Danh sách API Endpoints](#11-danh-sách-api-endpoints)
12. [Xử lý lỗi toàn cục](#12-xử-lý-lỗi-toàn-cục)
13. [Tích hợp dịch vụ ngoài](#13-tích-hợp-dịch-vụ-ngoài)

---

## 1. Tổng quan hệ thống

SmartCenter là nền tảng học trực tuyến hỗ trợ:

- **Student** đăng ký khóa học, học bài, làm bài thi, theo dõi tiến độ học tập.
- **Lecturer** tạo và quản lý khóa học, section, lesson, tài liệu, đề thi.
- **Staff** hỗ trợ vận hành: quản lý đơn hàng, tư vấn học viên, xử lý thanh toán.
- **Admin** quản trị toàn hệ thống: người dùng, phân quyền, audit log.

---

## 2. Kiến trúc dự án

Dự án theo mô hình **3-layer** tách biệt rõ ràng trách nhiệm:

```
SmartCenter.Api          ← Presentation Layer  (Controllers, Middleware, Extensions)
SmartCenter.Service      ← Business Logic Layer (Services, DTOs, Models)
SmartCenter.Repository   ← Data Access Layer    (Entities, DbContext, Migrations, Seed)
```

**Dependency flow:**

```
Api  →  Service  →  Repository
```

Mỗi tầng chỉ phụ thuộc vào tầng dưới nó, không phụ thuộc ngược chiều.

---

## 3. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Framework | .NET 8 / ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL (Npgsql provider) |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Task Scheduling | Quartz.NET |
| Email | MailKit (hỗ trợ SMTP: Gmail, Zoho) |
| File/Media Upload | Cloudinary (`CloudinaryDotNet`) |
| Payment | SePay (webhook-based) |
| API Docs | Swagger / Swashbuckle |
| Config | DotNetEnv (`.env` file support) |
| Containerization | Docker |

---

## 4. Cấu trúc thư mục

```
BE_SmartCenter/
│
├── SmartCenter.sln
│
├── SmartCenter.Api/                        # Presentation Layer
│   ├── Controllers/                        # 22 Controllers
│   ├── Middlewares/
│   │   └── GlobalExceptionHandlerMiddleware.cs
│   ├── extensions/
│   │   ├── JwtExtensions.cs               # JWT + Authorization Policies
│   │   └── SwaggerExtensions.cs           # Swagger UI config
│   ├── Dockerfile
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
├── SmartCenter.Service/                    # Business Logic Layer
│   ├── Auth/
│   ├── Admin/
│   ├── Cart/
│   ├── Combo/
│   ├── Comment/
│   ├── Course/ / CourseService/
│   ├── Document/
│   ├── EnrollmentService/
│   ├── ExamManagementService/
│   ├── ExamPaper/
│   ├── GradeService/
│   ├── JwtService/
│   ├── Lecture/
│   ├── Lesson/
│   ├── MailService/
│   ├── MediaService/ / CloudinaryService/
│   ├── Order/
│   ├── Payment/
│   ├── Progress/
│   ├── ReviewCourseService/
│   ├── Section/
│   ├── SePayService/
│   ├── Staff/
│   ├── Student/
│   ├── UserService/
│   ├── CategoryService/
│   ├── ConsultationService/
│   └── Model/                             # Shared response models (ApiResponse...)
│
└── SmartCenter.Repository/                # Data Access Layer
    ├── Abtraction/
    │   ├── BaseEntity.cs                  # BaseEntity<T> với Id, IsDeleted
    │   └── IAuditableEntity.cs            # CreatedAt, UpdatedAt
    ├── Entity/
    │   ├── Enums/                         # Các enum: UserRole, OrderStatus, ...
    │   └── *.cs                           # 30+ Entities
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── AppDbContextSeed.cs
    └── Migrations/
```

---

## 5. Domain & Entity

### Sơ đồ quan hệ chính

```
User ──┬── Student ──── Enrollment ──── Course
       ├── Lecturer ─── Section ──────── Lesson
       ├── Staff                         │
       └── Admin                         ├── Document
                                         ├── Comment (threaded)
                    Cart ────────────────┤
                     └── CartItem        └── ReviewCourse
                          ├── Course
                          └── Combo ──── ComboCourse ──── Course

Order ──── OrderItem
 └── Transaction

ExamManagement ──── ExamPaper ──── ExamPaperDetail ──── Question
                                                         ├── MultipleChoiceAnswer
                                                         └── EssayAnswer

LearningProcess ──── Deadline
```

### Danh sách Entity

| Entity | Mô tả |
|---|---|
| `User` | Tài khoản hệ thống, phân theo `UserRole` (Admin, Student, Lecturer, Staff) |
| `Student` | Profile học sinh, liên kết 1-1 với User |
| `Lecturer` | Profile giảng viên, liên kết 1-1 với User |
| `Course` | Khóa học (Online/Offline) |
| `Category` | Danh mục khóa học (many-to-many với Course qua `CourseCategory`) |
| `Section` | Chương/phần trong khóa học |
| `Lesson` | Bài học trong Section |
| `Document` | Tài liệu đính kèm của Lesson |
| `Comment` | Bình luận trên Lesson, hỗ trợ reply nhiều cấp (`DepthLevel`) |
| `ReviewCourse` | Đánh giá khóa học của Student sau khi học |
| `Enrollment` | Xác nhận học sinh đã đăng ký & thanh toán khóa học |
| `LearningProcess` | Theo dõi tiến độ học từng Lesson của Student |
| `Deadline` | Deadline bài tập / nộp bài trong khóa học |
| `Cart` | Giỏ hàng của Student |
| `CartItem` | Item trong giỏ hàng (Course hoặc Combo) |
| `Combo` | Gói combo nhiều khóa học với % giảm giá |
| `ComboCourse` | Bảng trung gian Combo ↔ Course |
| `Order` | Đơn hàng phát sinh từ giỏ hàng |
| `OrderItem` | Chi tiết từng sản phẩm trong Order |
| `Transaction` | Giao dịch thanh toán gắn với Order |
| `ExamManament` | Quản lý kỳ thi |
| `ExamPaper` | Đề thi |
| `ExamPaperDetail` | Chi tiết câu hỏi trong đề thi |
| `Question` | Câu hỏi (MultipleChoice / Essay) |
| `MultipleChoiceAnswer` | Đáp án trắc nghiệm |
| `EssayAnswer` | Bài làm tự luận của Student |
| `ConsultationRequest` | Yêu cầu tư vấn từ Student/User |
| `Notification` | Thông báo đẩy tới User |
| `AuditLog` | Log hành động của người dùng |
| `UserSession` | Phiên đăng nhập |

### Enums

| Enum | Giá trị |
|---|---|
| `UserRole` | `Admin = 1`, `Student`, `Lecturer`, `Staff` |
| `UserStatus` | `Active = 1`, `Inactive` |
| `CourseType` | `Online = 1`, `Offline` |
| `OrderStatus` | `Pending = 1`, `Paid`, `Cancelled`, `Expired` |
| `EnrollmentStatus` | `Paid = 1`, `Deposited` |
| `CartItemType` | `Course = 1`, `Combo` |
| `PaymentMethod` | `Cash = 1`, `BankTransfer` |
| `QuestionType` | `MultipleChoice = 1`, `Essay` |
| `ExamPaperStatus` | `Open = 1`, `Closed`, `Deleted` |
| `ConsultReqStatus` | `Pending = 1`, `Accepted`, `Rejected` |
| `DeadlineStatus` | `Processing = 1`, `Completed` |

---

## 6. Cấu hình môi trường

Tạo file `.env` tại thư mục gốc (hoặc chỉnh `appsettings.json`):

```env
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=smartcenter;Username=postgres;Password=yourpassword

# JWT
JwtOptions__SecretKey=your_secret_key_min_32_chars
JwtOptions__Issuer=smartcenter
JwtOptions__Audience=smartcenter
JwtOptions__ExpireMinutes=120

# Email (Gmail)
MailOptions__Mail=yourmail@gmail.com
MailOptions__DisplayName=SmartCenter
MailOptions__Password=your_app_password
MailOptions__Host=smtp.gmail.com
MailOptions__Port=587

# Cloudinary
CloudinaryOptions__CloudName=your_cloud_name
CloudinaryOptions__ApiKey=your_api_key
CloudinaryOptions__ApiSecret=your_api_secret

# SePay
SePayOptions__ApiKey=your_sepay_api_key
SePayOptions__WebhookToken=your_webhook_token
```

> **Lưu ý:** Gmail yêu cầu bật **App Password** (2FA). Xem hướng dẫn tại [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords).

---

## 7. Chạy dự án

### Yêu cầu

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 14+

### Các bước

```bash
# 1. Clone repository
git clone https://github.com/SmartCenter-System/BE_SmartCenter.git
cd BE_SmartCenter

# 2. Cấu hình biến môi trường
cp .env.example .env     # hoặc tạo file .env thủ công
# Điền thông tin vào .env

# 3. Restore packages
dotnet restore

# 4. Chạy ứng dụng (migration + seed chạy tự động khi khởi động)
dotnet run --project SmartCenter.Api
```

Ứng dụng khởi động tại:

- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

---

## 8. Chạy với Docker

```bash
# Build image
docker build -f SmartCenter.Api/Dockerfile -t smartcenter-api .

# Chạy container
docker run -d \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;..." \
  -e JwtOptions__SecretKey="your_secret" \
  --name smartcenter-api \
  smartcenter-api
```

Ứng dụng expose port `8080` (HTTP) và `8081` (HTTPS).

---

## 9. Database Migration & Seed Data

### Migration

Migration được áp dụng **tự động** khi ứng dụng khởi động thông qua:

```csharp
await db.Database.MigrateAsync();
```

Tạo migration mới thủ công:

```bash
dotnet ef migrations add <TênMigration> \
  --project SmartCenter.Repository \
  --startup-project SmartCenter.Api
```

### Seed Data

Seed Data cũng chạy tự động sau migration:

```csharp
await AppDbContextSeed.SeedAsync(db);
```

Mỗi method seed có **guard clause** — chỉ chạy nếu bảng chưa có dữ liệu:

| Method | Dữ liệu seed |
|---|---|
| `SeedUsersAsync` | Admin, Student, Lecturer, Staff mẫu |
| `SeedCoursesAsync` | Khóa học + Section + Lesson + Document |
| `SeedOrdersAndEnrollmentsAsync` | Đơn hàng + Enrollment |
| `SeedExamAsync` | Đề thi + Câu hỏi |
| `SeedLearningProcessAsync` | Tiến độ học tập |
| `SeedDocumentsAsync` | Tài liệu bài học |
| `SeedCategoriesAsync` | Danh mục khóa học |
| `SeedCombosAsync` | 4 Combo gói + ComboCourse + CartItem |
| `SeedReviewCoursesAsync` | Đánh giá khóa học (3–5★) |
| `SeedCommentsAsync` | 3–5 comment mỗi Lesson + reply |

---

## 10. Authentication & Authorization

### Luồng xác thực

```
Client  →  POST /api/auth/login  →  [AuthController]
                                        ↓
                               Validate credentials
                                        ↓
                               Issue JWT (Access Token)
                                        ↓
Client gửi header: Authorization: Bearer <token>
```

Token JWT chứa claims: `UserId`, `Role`, `Email`.

Thời hạn mặc định: **120 phút** (cấu hình qua `JwtOptions__ExpireMinutes`).

### Authorization Policies

| Policy | Roles được phép |
|---|---|
| `AdminPolicy` | Admin |
| `StaffPolicy` | Staff |
| `StudentPolicy` | Student |
| `LecturerPolicy` | Lecturer |
| `StaffOrAdminPolicy` | Staff, Admin |
| `AdminOrLecturerPolicy` | Admin, Lecturer |
| `LectureOrStudentPolicy` | Lecturer, Student |

Sử dụng trong Controller:

```csharp
[Authorize(Policy = JwtExtensions.AdminPolicy)]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id) { ... }
```

---

## 11. Danh sách API Endpoints

Tất cả endpoint có prefix `/api`. Truy cập Swagger để xem chi tiết request/response.

| Controller | Base Route | Mô tả chức năng |
|---|---|---|
| `AuthController` | `/api/auth` | Đăng ký, đăng nhập, xác thực email, đổi mật khẩu |
| `UserController` | `/api/user` | Xem & cập nhật thông tin cá nhân |
| `AdminController` | `/api/admin` | Quản trị người dùng, thống kê hệ thống |
| `StaffController` | `/api/staff` | Quản lý vận hành, hỗ trợ học viên |
| `LecturerController` | `/api/lecturer` | Quản lý thông tin giảng viên |
| `CoursesController` | `/api/courses` | CRUD khóa học |
| `CategoryController` | `/api/category` | CRUD danh mục |
| `SectionController` | `/api/section` | CRUD chương học |
| `LessonController` | `/api/lesson` | CRUD bài học |
| `DocumentController` | `/api/document` | Upload & quản lý tài liệu |
| `ComboController` | `/api/combo` | CRUD gói combo khóa học |
| `CartController` | `/api/cart` | Thêm/xóa item giỏ hàng |
| `OrderController` | `/api/order` | Tạo đơn hàng, xem lịch sử |
| `PaymentController` | `/api/payment` | Xử lý thanh toán, SePay webhook |
| `EnrollmentController` | `/api/enrollment` | Quản lý đăng ký khóa học |
| `ProgressController` | `/api/progress` | Theo dõi tiến độ học tập |
| `CommentController` | `/api/comment` | CRUD bình luận bài học |
| `ReviewCourseController` | `/api/reviewcourse` | Đánh giá khóa học |
| `ExamManagementController` | `/api/exammanagement` | Quản lý kỳ thi |
| `ExamPaperController` | `/api/exampaper` | CRUD đề thi, câu hỏi |
| `GradeExamController` | `/api/gradeexam` | Chấm điểm, xem kết quả |
| `ConsultationRequestController` | `/api/consultation` | Gửi & xử lý yêu cầu tư vấn |

> **Swagger UI:** `http://localhost:5000/swagger`  
> Tất cả endpoint yêu cầu xác thực đều cần header `Authorization: Bearer <token>`.

---

## 12. Xử lý lỗi toàn cục

`GlobalExceptionHandlerMiddleware` bắt toàn bộ exception chưa được xử lý và trả về response chuẩn:

```json
{
  "success": false,
  "message": "Mô tả lỗi cho client",
  "traceId": "0HN5A...",
  "errors": { "detail": "..." }   // Chỉ trả về ở môi trường Development
}
```

**Mapping HTTP Status Code:**

| Exception | HTTP Status |
|---|---|
| `ArgumentException` | 400 Bad Request |
| `InvalidOperationException` | 400 Bad Request |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `KeyNotFoundException` | 404 Not Found |
| Các exception khác | 500 Internal Server Error |

---

## 13. Tích hợp dịch vụ ngoài

### Cloudinary — Upload Media

Dùng để lưu trữ ảnh đại diện, thumbnail khóa học, video bài học.

```json
"CloudinaryOptions": {
  "CloudName": "...",
  "ApiKey": "...",
  "ApiSecret": "..."
}
```

### MailKit — Gửi Email

Hỗ trợ xác thực email khi đăng ký, gửi mã OTP reset mật khẩu, thông báo đơn hàng.

```json
"MailOptions": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Mail": "...",
  "Password": "..."
}
```

### SePay — Thanh toán

Xử lý thanh toán qua QR Code / chuyển khoản ngân hàng. Nhận kết quả qua webhook.

```json
"SePayOptions": {
  "ApiKey": "...",
  "WebhookToken": "..."
}
```

Endpoint webhook: `POST /api/payment/webhook`

### Quartz.NET — Scheduled Jobs

Dùng để xử lý các tác vụ nền định kỳ (ví dụ: tự động hủy đơn hàng `Expired`, gửi nhắc nhở deadline).

---

*README này được tạo dựa trên source code tại commit hiện tại. Cập nhật khi có thay đổi cấu trúc hệ thống.*
