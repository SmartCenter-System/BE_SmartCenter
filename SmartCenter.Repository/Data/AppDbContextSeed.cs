using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;
using System.Security.Cryptography;
using System.Text;
using Transaction = SmartCenter.Repository.Entity.Transaction;

namespace SmartCenter.Repository.Data;

public static class AppDbContextSeed
{
    private static readonly string VideoUrl = "https://youtu.be/KmiyIn6H_Kk?si=f3cpEINs4N8S9ePb";

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ENTRY POINT
    // ═══════════════════════════════════════════════════════════════════════

    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedUsersAsync(context);
        await SeedCoursesAsync(context);
        await SeedOrdersAndEnrollmentsAsync(context);
        await SeedExamAsync(context);
        await SeedLearningProcessAsync(context);
        await SeedDocumentsAsync(context);
        await SeedCategoriesAsync(context);
    }

    
    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;
    
        var now = DateTimeOffset.UtcNow;
    
        // ── Categories ────────────────────────────────────────────────────────
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Toán học",    Description = "Các khóa học Toán từ cơ bản đến nâng cao, luyện thi THPT Quốc gia.", IconUrl = "https://cdn.smartcenter.vn/icons/math.svg",    IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Vật lý",      Description = "Khóa học Vật lý cấp 3, bao gồm cơ học, điện học, quang học và hạt nhân.", IconUrl = "https://cdn.smartcenter.vn/icons/physics.svg", IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Hóa học",     Description = "Khóa học Hóa học từ đại cương đến hữu cơ, luyện thi THPT.", IconUrl = "https://cdn.smartcenter.vn/icons/chemistry.svg", IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Tiếng Anh",   Description = "Luyện ngữ pháp, từ vựng, 4 kỹ năng và thi THPT Quốc gia môn Anh.", IconUrl = "https://cdn.smartcenter.vn/icons/english.svg",  IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Ngữ văn",     Description = "Phân tích tác phẩm, kỹ năng nghị luận và luyện thi THPT môn Văn.", IconUrl = "https://cdn.smartcenter.vn/icons/literature.svg",IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Luyện thi",   Description = "Các khóa ôn thi THPT Quốc gia toàn diện theo cấu trúc đề thi mới nhất.", IconUrl = "https://cdn.smartcenter.vn/icons/exam.svg",      IsActive = true, CreatedAt = now },
        };
    
        await context.Categories.AddRangeAsync(categories);
    
        // ── CourseCategories — gán category cho từng course ───────────────────
        var courses = await context.Courses.ToListAsync();
    
        // Map tên course → category
        var courseCategories = new List<CourseCategory>();
    
        foreach (var course in courses)
        {
            var matchedCategoryIds = new List<Guid>();
    
            // Gán category chính theo môn
            if (course.CourseName.Contains("Toán"))
                matchedCategoryIds.Add(categories[0].Id); // Toán học
    
            if (course.CourseName.Contains("Vật lý"))
                matchedCategoryIds.Add(categories[1].Id); // Vật lý
    
            if (course.CourseName.Contains("Hóa"))
                matchedCategoryIds.Add(categories[2].Id); // Hóa học
    
            if (course.CourseName.Contains("Tiếng Anh") || course.CourseName.Contains("Anh"))
                matchedCategoryIds.Add(categories[3].Id); // Tiếng Anh
    
            if (course.CourseName.Contains("Ngữ văn") || course.CourseName.Contains("Văn"))
                matchedCategoryIds.Add(categories[4].Id); // Ngữ văn
    
            if (course.CourseName.Contains("Luyện thi") || course.CourseName.Contains("THPT"))
                matchedCategoryIds.Add(categories[5].Id); // Luyện thi
    
            foreach (var catId in matchedCategoryIds.Distinct())
            {
                courseCategories.Add(new CourseCategory
                {
                    Id         = Guid.NewGuid(),
                    CourseId   = course.Id,
                    CategoryId = catId,
                    CreatedAt  = now,
                });
            }
        }
    
        await context.CourseCategories.AddRangeAsync(courseCategories);
        await context.SaveChangesAsync();
    }
    // ═══════════════════════════════════════════════════════════════════════
    // 1. USERS
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var now = DateTimeOffset.UtcNow;

        var adminUser = MakeUser("SmartCenter", "Admin", "admin@smartcenter.vn",
            "0961002445", UserRole.Admin, "Admin@123", now);
        await context.Users.AddAsync(adminUser);

        var lecUserData = new[]
        {
            ("Trần",   "Thị Minh Châu",  "minhchau@smartcenter.vn",  "0912345001", "Toán học"),
            ("Lê",     "Văn Hùng",       "vanhung@smartcenter.vn",   "0912345002", "Vật lý"),
            ("Phạm",   "Thị Thu Hương",  "thuhuong@smartcenter.vn",  "0912345003", "Hóa học"),
            ("Võ",     "Quang Minh",     "quangminh@smartcenter.vn", "0912345004", "Tiếng Anh"),
            ("Nguyễn", "Thị Lan Anh",    "lananh@smartcenter.vn",    "0912345005", "Ngữ văn"),
        };

        var lecUsers = lecUserData.Select(d =>
            MakeUser(d.Item1, d.Item2, d.Item3, d.Item4, UserRole.Lecturer, "Lecturer@123", now)
        ).ToList();
        await context.Users.AddRangeAsync(lecUsers);

        var lecturers = lecUsers.Zip(lecUserData, (u, d) => new Lecturer
        {
            Id        = Guid.NewGuid(),
            UserId    = u.Id,
            Expertise = d.Item5,
            Bio       = $"Giảng viên {d.Item5} với nhiều năm kinh nghiệm giảng dạy cấp THPT.",
            CreatedAt = now,
        }).ToList();
        await context.Lecturers.AddRangeAsync(lecturers);

        var stuUserData = new[]
        {
            ("Hoàng",  "Minh Tuấn",     "minhtuan@gmail.com",   "0923456001"),
            ("Nguyễn", "Thị Thùy Linh", "thuylinh@gmail.com",   "0923456002"),
            ("Trần",   "Văn Khánh",     "vankhanh@gmail.com",   "0923456003"),
            ("Lê",     "Thị Ngọc Hân",  "ngochan@gmail.com",    "0923456004"),
            ("Phạm",   "Đức Vinh",      "ducvinh@gmail.com",    "0923456005"),
            ("Bùi",    "Thị Thảo Nhi",  "thaonhi@gmail.com",    "0923456006"),
            ("Võ",     "Thành Liêm",    "thanhliem@gmail.com",  "0923456007"),
            ("Đặng",   "Thị Mỹ Duyên",  "myduyen@gmail.com",    "0923456008"),
            ("Nguyễn", "Hoàng Lộc",     "hoangloc@gmail.com",   "0923456009"),
            ("Trịnh",  "Thị Kim Ánh",   "kimanh@gmail.com",     "0923456010"),
        };

        var stuUsers = stuUserData.Select(d =>
            MakeUser(d.Item1, d.Item2, d.Item3, d.Item4, UserRole.Student, "Student@123", now)
        ).ToList();
        await context.Users.AddRangeAsync(stuUsers);

        var carts = stuUsers.Select(_ => new Cart { Id = Guid.NewGuid(), StuId = Guid.Empty }).ToList();
        await context.Carts.AddRangeAsync(carts);

        var students = stuUsers.Select((u, i) =>
        {
            var student = new Student
            {
                Id             = Guid.NewGuid(),
                UserId         = u.Id,
                CartId         = carts[i].Id,
                Address        = $"Số {(i + 1) * 10} Đường Lê Lợi, Quận {i % 5 + 1}",
                City           = i % 2 == 0 ? "TP. Hồ Chí Minh" : "Hà Nội",
                EnrollmentDate = now,
                CreatedAt      = now,
            };
            carts[i].StuId = student.Id;
            return student;
        }).ToList();
        await context.Students.AddRangeAsync(students);

        await context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 2. COURSES + SECTIONS + LESSONS + CART ITEMS
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task SeedCoursesAsync(AppDbContext context)
    {
        if (await context.Courses.AnyAsync()) return;

        var now       = DateTimeOffset.UtcNow;
        var lecturers = await context.Lecturers.ToListAsync();
        var carts     = await context.Carts.ToListAsync();

        var courses = new List<Course>
        {
            MakeCourse(lecturers[0].Id, "Toán 10 — Đại số và Giải tích",
                "Nắm vững chương trình Toán lớp 10: hàm số, phương trình, bất phương trình, lượng giác.",
                350000, CourseType.Online, 50, now),
            MakeCourse(lecturers[0].Id, "Toán 12 — Luyện thi THPT Quốc gia",
                "Ôn tập toàn bộ chương trình Toán 12, giải đề thi thử và phân tích đề thi thật.",
                450000, CourseType.Online, 60, now),
            MakeCourse(lecturers[1].Id, "Vật lý 11 — Điện học & Quang học",
                "Điện tích, điện trường, dòng điện, từ trường và khúc xạ ánh sáng.",
                320000, CourseType.Online, 40, now),
            MakeCourse(lecturers[1].Id, "Vật lý 12 — Luyện thi THPT Quốc gia",
                "Dao động, sóng, điện xoay chiều, lượng tử ánh sáng, hạt nhân nguyên tử.",
                420000, CourseType.Online, 50, now),
            MakeCourse(lecturers[2].Id, "Hóa học 10 — Hóa đại cương",
                "Nguyên tử, bảng tuần hoàn, liên kết hóa học, phản ứng oxi hóa - khử.",
                300000, CourseType.Online, 45, now),
            MakeCourse(lecturers[2].Id, "Hóa học 12 — Luyện thi THPT Quốc gia",
                "Este, lipit, cacbohiđrat, amin, aminoaxit, polime, kim loại và hợp chất.",
                400000, CourseType.Online, 55, now),
            MakeCourse(lecturers[3].Id, "Tiếng Anh 10 — Grammar & Vocabulary",
                "Ngữ pháp và từ vựng tiếng Anh lớp 10 theo chương trình mới, luyện 4 kỹ năng.",
                280000, CourseType.Online, 70, now),
            MakeCourse(lecturers[3].Id, "Luyện thi THPT — Tiếng Anh",
                "Chiến lược làm bài thi THPT Quốc gia môn Tiếng Anh, luyện đề theo cấu trúc mới nhất.",
                380000, CourseType.Online, 80, now),
            MakeCourse(lecturers[4].Id, "Ngữ văn 11 — Văn học Việt Nam",
                "Phân tích tác phẩm văn học Việt Nam lớp 11, kỹ năng viết nghị luận văn học.",
                250000, CourseType.Online, 60, now),
            MakeCourse(lecturers[4].Id, "Ngữ văn 12 — Luyện thi THPT Quốc gia",
                "Ôn tập đọc hiểu, nghị luận xã hội, nghị luận văn học theo cấu trúc đề thi mới.",
                350000, CourseType.Online, 70, now),
        };
        await context.Courses.AddRangeAsync(courses);

        var allSections = new List<Section>();
        var allLessons  = new List<Lesson>();

        void AddContent(Course course, (string SecTitle, string[] LessonTitles)[] data)
        {
            for (int si = 0; si < data.Length; si++)
            {
                var sec = new Section
                {
                    Id       = Guid.NewGuid(),
                    CourseId = course.Id,
                    Title    = data[si].SecTitle,
                    Position = si + 1,
                    IsActive = true,
                };
                allSections.Add(sec);
                for (int li = 0; li < data[si].LessonTitles.Length; li++)
                {
                    allLessons.Add(new Lesson
                    {
                        Id          = Guid.NewGuid(),
                        SectionId   = sec.Id,
                        CourseId    = course.Id,
                        Title       = data[si].LessonTitles[li],
                        Description = $"Nội dung bài: {data[si].LessonTitles[li]}",
                        VideoUrl    = VideoUrl,
                        Duration    = 30 + (li * 5),
                        Position    = li + 1,
                        IsPreview   = li == 0,
                        CreatedAt   = now,
                    });
                }
            }
        }

        AddContent(courses[0], new[]
        {
            ("Mệnh đề & Tập hợp",               new[] { "Mệnh đề và các phép toán", "Tập hợp và các phép toán tập hợp", "Số gần đúng và sai số" }),
            ("Hàm số bậc nhất & bậc hai",        new[] { "Hàm số và đồ thị", "Hàm số bậc nhất", "Hàm số bậc hai", "Vẽ đồ thị parabol" }),
            ("Phương trình & Hệ PT",             new[] { "Phương trình bậc nhất, bậc hai", "Hệ phương trình bậc nhất hai ẩn", "Phương trình quy về bậc hai" }),
            ("Bất phương trình",                 new[] { "Bất đẳng thức", "Bất phương trình bậc nhất", "Dấu nhị thức và tam thức bậc hai" }),
            ("Lượng giác",                       new[] { "Cung và góc lượng giác", "Giá trị lượng giác", "Công thức lượng giác", "Phương trình lượng giác cơ bản" }),
        });
        AddContent(courses[1], new[]
        {
            ("Ứng dụng đạo hàm",        new[] { "Tính đơn điệu của hàm số", "Cực trị của hàm số", "Giá trị lớn nhất - nhỏ nhất", "Tiệm cận" }),
            ("Hàm số mũ & Logarit",     new[] { "Hàm số mũ và đồ thị", "Hàm số logarit", "Phương trình mũ - logarit", "Bất phương trình mũ - logarit" }),
            ("Nguyên hàm & Tích phân",  new[] { "Nguyên hàm cơ bản", "Tích phân xác định", "Tính diện tích bằng tích phân", "Tính thể tích bằng tích phân" }),
            ("Số phức",                 new[] { "Khái niệm số phức", "Các phép toán số phức", "Phương trình bậc hai nghiệm phức" }),
            ("Luyện đề tổng hợp",       new[] { "Phân tích cấu trúc đề thi", "Luyện đề thử số 1", "Luyện đề thử số 2", "Giải đề thi thật 2024" }),
        });
        AddContent(courses[2], new[]
        {
            ("Điện tích & Điện trường", new[] { "Điện tích - Định luật Coulomb", "Điện trường", "Công của lực điện", "Điện thế và hiệu điện thế" }),
            ("Tụ điện & Dòng điện",     new[] { "Tụ điện", "Dòng điện không đổi", "Nguồn điện - Định luật Ohm", "Định luật Kirchhoff" }),
            ("Từ trường",               new[] { "Từ trường và đường sức từ", "Lực từ - Lực Lorentz", "Cảm ứng điện từ", "Tự cảm" }),
            ("Quang hình học",          new[] { "Khúc xạ ánh sáng", "Phản xạ toàn phần", "Lăng kính", "Thấu kính" }),
        });
        AddContent(courses[3], new[]
        {
            ("Dao động cơ",             new[] { "Dao động điều hoà", "Con lắc lò xo", "Con lắc đơn", "Tổng hợp dao động" }),
            ("Sóng cơ & Sóng âm",       new[] { "Sóng cơ học", "Giao thoa sóng", "Sóng dừng", "Đặc trưng vật lý sóng âm" }),
            ("Điện xoay chiều",         new[] { "Mạch RLC nối tiếp", "Cộng hưởng điện", "Công suất điện xoay chiều", "Máy biến áp" }),
            ("Hạt nhân nguyên tử",      new[] { "Cấu tạo hạt nhân", "Phóng xạ", "Phản ứng hạt nhân", "Năng lượng hạt nhân" }),
            ("Luyện đề tổng hợp",       new[] { "Chiến lược làm bài thi", "Đề thử số 1", "Đề thử số 2", "Giải đề thi thật 2024" }),
        });
        AddContent(courses[4], new[]
        {
            ("Nguyên tử",               new[] { "Thành phần nguyên tử", "Hạt nhân và vỏ electron", "Cấu hình electron", "Đồng vị" }),
            ("Bảng tuần hoàn",          new[] { "Cấu tạo bảng tuần hoàn", "Xu hướng biến đổi tuần hoàn", "Ý nghĩa bảng tuần hoàn" }),
            ("Liên kết hóa học",        new[] { "Liên kết ion", "Liên kết cộng hóa trị", "Liên kết kim loại", "Hiệu độ âm điện" }),
            ("Phản ứng oxi hóa - khử",  new[] { "Số oxi hóa", "Phản ứng oxi hóa - khử", "Cân bằng phản ứng theo electron", "Ứng dụng thực tiễn" }),
        });
        AddContent(courses[5], new[]
        {
            ("Este & Lipit",                new[] { "Khái niệm và tính chất este", "Phản ứng xà phòng hóa", "Chất béo", "Bài toán hỗn hợp este" }),
            ("Cacbohiđrat",                 new[] { "Glucozơ", "Saccarozơ", "Tinh bột và xenlulozơ", "Bài toán tổng hợp" }),
            ("Amin - Aminoaxit - Protein",  new[] { "Amin và tính chất", "Aminoaxit và muối amoni", "Peptit và protein", "Bài tập tổng hợp" }),
            ("Kim loại",                    new[] { "Tính chất chung kim loại", "Kim loại kiềm và kiềm thổ", "Nhôm và hợp chất", "Sắt và hợp chất" }),
            ("Luyện đề tổng hợp",           new[] { "Phân tích đề và chiến lược", "Đề thử số 1", "Đề thử số 2", "Giải đề thi thật 2024" }),
        });
        AddContent(courses[6], new[]
        {
            ("Ngữ pháp nền tảng",   new[] { "Thì hiện tại đơn & tiếp diễn", "Thì quá khứ đơn & tiếp diễn", "Thì tương lai", "Câu điều kiện" }),
            ("Từ vựng theo chủ đề", new[] { "Family & Relationships", "Education & School", "Environment", "Technology" }),
            ("Kỹ năng Reading",     new[] { "Đọc hiểu điền từ", "Đọc hiểu trả lời câu hỏi", "Chiến lược skimming & scanning" }),
            ("Kỹ năng Writing",     new[] { "Viết đoạn văn cơ bản", "Viết thư", "Viết bài luận ngắn" }),
        });
        AddContent(courses[7], new[]
        {
            ("Ngữ pháp trọng tâm",  new[] { "Mệnh đề quan hệ", "Câu bị động", "Câu gián tiếp", "Từ nối và liên từ" }),
            ("Từ vựng thi THPT",    new[] { "Từ đồng nghĩa - trái nghĩa", "Thành ngữ thông dụng", "Word form", "Collocations" }),
            ("Kỹ năng làm bài thi", new[] { "Phần Pronunciation", "Phần Grammar & Vocabulary", "Phần Reading comprehension", "Phần Writing" }),
            ("Luyện đề",            new[] { "Đề thử số 1 có giải", "Đề thử số 2 có giải", "Phân tích lỗi sai thường gặp", "Giải đề thi thật 2024" }),
        });
        AddContent(courses[8], new[]
        {
            ("Văn học trung đại VN",    new[] { "Vào phủ chúa Trịnh - Lê Hữu Trác", "Tự tình II - Hồ Xuân Hương", "Câu cá mùa thu - Nguyễn Khuyến", "Thương vợ - Trần Tế Xương" }),
            ("Thơ lãng mạn 1930-1945", new[] { "Vội vàng - Xuân Diệu", "Đây thôn Vĩ Dạ - Hàn Mặc Tử", "Tràng giang - Huy Cận", "Chiều tối - Hồ Chí Minh" }),
            ("Văn xuôi hiện đại",       new[] { "Hai đứa trẻ - Thạch Lam", "Chữ người tử tù - Nguyễn Tuân", "Chí Phèo - Nam Cao" }),
            ("Kỹ năng viết nghị luận",  new[] { "Nghị luận về tư tưởng đạo lý", "Nghị luận về hiện tượng xã hội", "Nghị luận về một đoạn thơ", "Nghị luận về tác phẩm văn xuôi" }),
        });
        AddContent(courses[9], new[]
        {
            ("Đọc hiểu",            new[] { "Phương thức biểu đạt", "Biện pháp tu từ", "Nội dung và ý nghĩa văn bản", "Chiến lược làm phần đọc hiểu" }),
            ("Nghị luận xã hội",    new[] { "Cách viết đoạn văn 200 chữ", "Dạng tư tưởng đạo lý", "Dạng hiện tượng xã hội", "Luyện viết có nhận xét" }),
            ("Nghị luận văn học",   new[] { "Tây Tiến - Quang Dũng", "Việt Bắc - Tố Hữu", "Đất Nước - Nguyễn Khoa Điềm", "Sóng - Xuân Quỳnh", "Chiếc thuyền ngoài xa - Nguyễn Minh Châu" }),
            ("Luyện đề tổng hợp",   new[] { "Phân tích cấu trúc đề thi", "Đề thử số 1", "Đề thử số 2", "Giải đề thi thật 2024" }),
        });

        await context.Sections.AddRangeAsync(allSections);
        await context.Lessons.AddRangeAsync(allLessons);

        var cartItems = new List<CartItem>
        {
            new() { Id = Guid.NewGuid(), CartId = carts[0].Id, CourseId = courses[0].Id, ItemType = CartItemType.Course, Quantity = 1, CreatedAt = now },
            new() { Id = Guid.NewGuid(), CartId = carts[0].Id, CourseId = courses[2].Id, ItemType = CartItemType.Course, Quantity = 1, CreatedAt = now },
            new() { Id = Guid.NewGuid(), CartId = carts[1].Id, CourseId = courses[1].Id, ItemType = CartItemType.Course, Quantity = 1, CreatedAt = now },
            new() { Id = Guid.NewGuid(), CartId = carts[2].Id, CourseId = courses[4].Id, ItemType = CartItemType.Course, Quantity = 1, CreatedAt = now },
            new() { Id = Guid.NewGuid(), CartId = carts[3].Id, CourseId = courses[6].Id, ItemType = CartItemType.Course, Quantity = 1, CreatedAt = now },
            new() { Id = Guid.NewGuid(), CartId = carts[3].Id, CourseId = courses[7].Id, ItemType = CartItemType.Course, Quantity = 1, CreatedAt = now },
            new() { Id = Guid.NewGuid(), CartId = carts[4].Id, CourseId = courses[8].Id, ItemType = CartItemType.Course, Quantity = 1, CreatedAt = now },
        };
        await context.CartItems.AddRangeAsync(cartItems);
        await context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 3. ORDERS + TRANSACTIONS + ORDER ITEMS + ENROLLMENTS
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task SeedOrdersAndEnrollmentsAsync(AppDbContext context)
    {
        if (await context.Orders.AnyAsync()) return;

        var now      = DateTimeOffset.UtcNow;
        var students = await context.Students.ToListAsync();
        var courses  = await context.Courses.ToListAsync();
        var admin    = await context.Users.FirstAsync(u => u.Role == UserRole.Admin);

        var purchasePlan = new[]
        {
            (students[5], new[] { courses[0], courses[1] }),
            (students[6], new[] { courses[2], courses[3] }),
            (students[7], new[] { courses[4], courses[5] }),
            (students[8], new[] { courses[6], courses[7] }),
            (students[9], new[] { courses[8], courses[9] }),
        };

        long orderCodeBase = now.Ticks;
        foreach (var (student, boughtCourses) in purchasePlan)
        {
            var subtotal = boughtCourses.Sum(c => c.BasePrice);
            var order = new Order
            {
                Id             = Guid.NewGuid(),
                StuId          = student.Id,
                OrderCode      = $"ORD{orderCodeBase++}",
                Status         = OrderStatus.Paid,
                PaymentMethod  = PaymentMethod.BankTransfer,
                SubtotalAmount = subtotal,
                DiscountAmount = 0,
                TotalAmount    = subtotal,
                CreatedAt      = now,
                UpdatedAt      = now,
                ExpireAt       = now.AddMinutes(15),
                PaidAt         = now,
            };
            await context.Orders.AddAsync(order);

            var transaction = new Transaction
            {
                Id                      = Guid.NewGuid(),
                OrderId                 = order.Id,
                Amount                  = subtotal,
                Status                  = "Full_Complete",
                ProviderTransactionCode = $"TXN{orderCodeBase}",
                ConfirmedByStaffId      = admin.Id,
                ConfirmedAt             = now,
                CreatedAt               = now,
            };
            await context.Transactions.AddAsync(transaction);

            foreach (var course in boughtCourses)
            {
                await context.OrderItems.AddAsync(new OrderItem
                {
                    Id        = Guid.NewGuid(),
                    OrderId   = order.Id,
                    CourseId  = course.Id,
                    ItemName  = course.CourseName,
                    UnitPrice = course.BasePrice,
                    Quantity  = 1,
                    CreatedAt = now,
                });

                await context.Enrollments.AddAsync(new Enrollment
                {
                    Id             = Guid.NewGuid(),
                    CourseId       = course.Id,
                    StuId          = student.Id,
                    Status         = EnrollmentStatus.Paid,
                    EnrollmentDate = now,
                    TransactionId  = transaction.Id,
                    CreatedAt      = now,
                });
            }
        }

        await context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 4. EXAM (30-40 câu TN thuần)
    //    Questions → MultipleChoiceAnswers → ExamPaper → Deadline
    //    → ExamPaperDetail → ExamManagement → ExamManagementDetail
    //    → ExamComment
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task SeedExamAsync(AppDbContext context)
    {
        if (await context.ExamPapers.AnyAsync()) return;

        var now       = DateTimeOffset.UtcNow;
        var lecturers = await context.Lecturers.ToListAsync();
        var students  = await context.Students.ToListAsync();
        var courses   = await context.Courses.ToListAsync();
        var lessons   = await context.Lessons.ToListAsync();

        // ── Ngân hàng câu hỏi trắc nghiệm — 10 bộ × 30 câu ─────────────
        // Mỗi môn 30 câu, điểm 10/30 ≈ 0.33đ/câu

        var questionBanks = new Dictionary<int, (string Q, string Correct, string[] Wrong)[]>
        {
            // ── Toán 10 (30 câu) ─────────────────────────────────────────
            [0] = new[]
            {
                ("Hàm số y = 2x + 3 là hàm số:", "Bậc nhất", new[]{ "Bậc hai", "Bậc ba", "Hằng số" }),
                ("Tập xác định của y = √(x-1) là:", "[1; +∞)", new[]{ "(1; +∞)", "(-∞; 1]", "ℝ" }),
                ("Phương trình x² - 4 = 0 có nghiệm:", "x = ±2", new[]{ "x = 2", "x = -2", "Vô nghiệm" }),
                ("Biệt thức Δ = b² - 4ac của x² - 5x + 6 = 0 là:", "1", new[]{ "-1", "4", "0" }),
                ("Hệ số góc của đường thẳng y = 3x - 7 là:", "3", new[]{ "-7", "-3", "7" }),
                ("Parabol y = x² có đỉnh tại:", "(0; 0)", new[]{ "(1; 0)", "(0; 1)", "(-1; 0)" }),
                ("Tập nghiệm của bất phương trình 2x - 4 > 0 là:", "(2; +∞)", new[]{ "(-∞; 2)", "[2; +∞)", "(-∞; 2]" }),
                ("sin(90°) bằng:", "1", new[]{ "0", "-1", "√2/2" }),
                ("cos(0°) bằng:", "1", new[]{ "0", "-1", "√3/2" }),
                ("tan(45°) bằng:", "1", new[]{ "√3", "√2", "0" }),
                ("Trong tam giác vuông, sin = ?", "Cạnh đối / Cạnh huyền", new[]{ "Cạnh kề / Cạnh huyền", "Cạnh đối / Cạnh kề", "Cạnh huyền / Cạnh đối" }),
                ("Công thức sin(A+B) = ?", "sinA·cosB + cosA·sinB", new[]{ "sinA·sinB + cosA·cosB", "sinA·cosB - cosA·sinB", "cosA·cosB - sinA·sinB" }),
                ("Phương trình log₂(x) = 3 có nghiệm x =", "8", new[]{ "6", "9", "3" }),
                ("Mệnh đề phủ định của 'x > 0' là:", "x ≤ 0", new[]{ "x < 0", "x = 0", "x ≥ 0" }),
                ("A ∪ B khi A = {1,2}, B = {2,3} là:", "{1,2,3}", new[]{ "{2}", "{1,3}", "{1,2}" }),
                ("A ∩ B khi A = {1,2}, B = {2,3} là:", "{2}", new[]{ "{1,2,3}", "{1,3}", "{}" }),
                ("Số nghiệm của x² + 2x + 5 = 0 là:", "0 nghiệm thực", new[]{ "1 nghiệm thực", "2 nghiệm thực", "Vô số nghiệm" }),
                ("Hàm số y = x² - 4x + 3 có giá trị nhỏ nhất tại:", "x = 2", new[]{ "x = 1", "x = 3", "x = -2" }),
                ("Giá trị nhỏ nhất của y = x² - 4x + 3 là:", "-1", new[]{ "0", "3", "1" }),
                ("Số gần đúng 3.14159 làm tròn đến phần trăm là:", "3.14", new[]{ "3.1", "3.142", "3.15" }),
                ("Hệ phương trình x+y=5, x-y=1 có nghiệm:", "x=3, y=2", new[]{ "x=2, y=3", "x=4, y=1", "x=1, y=4" }),
                ("Phương trình |x - 2| = 3 có nghiệm:", "x = 5 hoặc x = -1", new[]{ "x = 5", "x = -1", "x = 1 hoặc x = 5" }),
                ("Giá trị của sin²(x) + cos²(x) = ?", "1", new[]{ "0", "2", "sin(2x)" }),
                ("Điều kiện để ax² + bx + c = 0 có 2 nghiệm phân biệt:", "Δ > 0", new[]{ "Δ = 0", "Δ < 0", "a ≠ 0" }),
                ("Tổng hai nghiệm của x² - 7x + 10 = 0 theo Vi-ét là:", "7", new[]{ "10", "-7", "-10" }),
                ("Tích hai nghiệm của x² - 7x + 10 = 0 theo Vi-ét là:", "10", new[]{ "7", "-10", "-7" }),
                ("Đồ thị hàm bậc hai y = ax² + bx + c mở lên khi:", "a > 0", new[]{ "a < 0", "b > 0", "c > 0" }),
                ("cos(60°) bằng:", "1/2", new[]{ "√3/2", "√2/2", "1" }),
                ("sin(30°) bằng:", "1/2", new[]{ "√3/2", "√2/2", "1" }),
                ("Phương trình sin(x) = 0 có nghiệm tổng quát:", "x = kπ (k ∈ ℤ)", new[]{ "x = π/2 + kπ", "x = 2kπ", "x = k·90°" }),
            },

            // ── Toán 12 (30 câu) ──────────────────────────────────────────
            [1] = new[]
            {
                ("Hàm số y = x³ - 3x + 2 đồng biến trên:", "(-∞;-1) và (1;+∞)", new[]{ "(-1;1)", "(-∞;0)", "(0;+∞)" }),
                ("Cực đại của y = -x² + 4x - 1 bằng:", "3", new[]{ "4", "2", "5" }),
                ("log₂8 bằng:", "3", new[]{ "4", "2", "8" }),
                ("Nguyên hàm của f(x) = 2x là:", "x² + C", new[]{ "2x² + C", "x + C", "2 + C" }),
                ("∫₀¹ x dx bằng:", "1/2", new[]{ "1", "0", "2" }),
                ("|2 + 3i| bằng:", "√13", new[]{ "5", "√5", "√7" }),
                ("Nghiệm của 2^x = 8:", "x = 3", new[]{ "x = 4", "x = 2", "x = 8" }),
                ("Đạo hàm của y = eˣ là:", "eˣ", new[]{ "xeˣ⁻¹", "eˣ⁻¹", "x·eˣ" }),
                ("Tiệm cận ngang của y=(2x+1)/(x-1):", "y = 2", new[]{ "y = 1", "y = -1", "y = 0" }),
                ("Thể tích khối cầu bán kính R:", "4πR³/3", new[]{ "πR³", "2πR³", "4πR²" }),
                ("x² + 1 = 0 có bao nhiêu nghiệm thực?", "0", new[]{ "1", "2", "-1" }),
                ("y = ln(x) xác định khi:", "x > 0", new[]{ "x ≥ 0", "x ∈ ℝ", "x ≠ 0" }),
                ("Đạo hàm của y = sin(x):", "cos(x)", new[]{ "-sin(x)", "-cos(x)", "tan(x)" }),
                ("Cực tiểu của y = x⁴ - 2x² + 1 tại:", "x = ±1", new[]{ "x = 0", "x = 2", "x = -2" }),
                ("∫ sin(x) dx = ?", "-cos(x) + C", new[]{ "cos(x) + C", "sin(x)+C", "-sin(x)+C" }),
                ("log₃(x) = 2 → x = ?", "9", new[]{ "6", "3", "27" }),
                ("Tổng nghiệm x²-5x+6=0:", "5", new[]{ "6", "-5", "-6" }),
                ("Diện tích giới hạn y=x² và y=x:", "1/6", new[]{ "1/2", "1/3", "1/4" }),
                ("Số phức liên hợp của 3-2i:", "3+2i", new[]{ "-3+2i", "3-2i", "-3-2i" }),
                ("lim(x→∞)(3x²+1)/(x²-2)=?", "3", new[]{ "0", "∞", "1" }),
                ("Đạo hàm của y = cos(x):", "-sin(x)", new[]{ "sin(x)", "cos(x)", "-cos(x)" }),
                ("Đạo hàm của y = xⁿ là:", "n·xⁿ⁻¹", new[]{ "xⁿ⁺¹/(n+1)", "n·xⁿ", "(n-1)·xⁿ" }),
                ("∫ eˣ dx = ?", "eˣ + C", new[]{ "eˣ⁻¹ + C", "x·eˣ + C", "eˣ/x + C" }),
                ("Số phức i² = ?", "-1", new[]{ "1", "i", "-i" }),
                ("Hàm số y = aˣ (a>1) là hàm:", "Đồng biến trên ℝ", new[]{ "Nghịch biến trên ℝ", "Không đơn điệu", "Đồng biến trên (0;+∞)" }),
                ("Đạo hàm của y = ln(x) là:", "1/x", new[]{ "ln(x)/x", "x", "1/x²" }),
                ("∫₀^π cos(x) dx = ?", "0", new[]{ "1", "-1", "2" }),
                ("Phần thực của z = 4 + 5i là:", "4", new[]{ "5", "4+5i", "√41" }),
                ("Phần ảo của z = 4 + 5i là:", "5", new[]{ "4", "5i", "√41" }),
                ("y = x² - 6x + 10 có giá trị nhỏ nhất:", "1", new[]{ "0", "10", "-1" }),
            },

            // ── Vật lý 11 (30 câu) ────────────────────────────────────────
            [2] = new[]
            {
                ("Đơn vị của điện tích là:", "Coulomb (C)", new[]{ "Ampe (A)", "Vôn (V)", "Fara (F)" }),
                ("Định luật Coulomb: F = ?", "k·|q₁·q₂|/r²", new[]{ "k·q₁·q₂·r²", "k·(q₁+q₂)/r²", "k·q₁·q₂/r" }),
                ("Điện trường là đại lượng:", "Vectơ", new[]{ "Vô hướng", "Tensor", "Không xác định" }),
                ("Đơn vị của cường độ điện trường:", "V/m", new[]{ "N", "C", "J" }),
                ("Tụ điện dùng để:", "Tích trữ điện tích", new[]{ "Dẫn điện", "Chuyển đổi điện", "Đo điện" }),
                ("Điện dung của tụ phẳng phụ thuộc:", "Diện tích bản, khoảng cách", new[]{ "Điện tích nạp vào", "Hiệu điện thế", "Vật liệu dẫn" }),
                ("Định luật Ohm: I = ?", "U/R", new[]{ "U·R", "R/U", "U²/R" }),
                ("Đơn vị điện trở:", "Ohm (Ω)", new[]{ "Ampe (A)", "Vôn (V)", "Watt (W)" }),
                ("Điện năng tiêu thụ: A = ?", "U·I·t", new[]{ "U·I", "I²·R", "P/t" }),
                ("Từ trường tạo ra bởi:", "Dòng điện và nam châm", new[]{ "Điện tích đứng yên", "Tụ điện", "Điện trở" }),
                ("Đơn vị của từ thông:", "Weber (Wb)", new[]{ "Tesla (T)", "Henry (H)", "Fara (F)" }),
                ("Lực Lorentz tác dụng lên:", "Điện tích chuyển động trong từ trường", new[]{ "Nam châm", "Dây dẫn đứng yên", "Tụ điện" }),
                ("Hiện tượng khúc xạ xảy ra khi:", "Ánh sáng truyền qua mặt phân cách 2 môi trường", new[]{ "Ánh sáng phản xạ", "Ánh sáng bị hấp thụ", "Ánh sáng nhiễu xạ" }),
                ("Điều kiện phản xạ toàn phần:", "i ≥ i_gh và n₁ > n₂", new[]{ "i < i_gh", "n₁ < n₂", "Mọi góc tới" }),
                ("Thấu kính hội tụ có tiêu cự:", "f > 0", new[]{ "f < 0", "f = 0", "f = ∞" }),
                ("Công thức thấu kính: 1/f = ?", "1/d + 1/d'", new[]{ "d + d'", "d·d'", "1/d - 1/d'" }),
                ("Hiệu điện thế đơn vị:", "Vôn (V)", new[]{ "Ampe (A)", "Watt (W)", "Ohm (Ω)" }),
                ("Công suất điện: P = ?", "U·I", new[]{ "U/I", "U+I", "U-I" }),
                ("Điện thế tại một điểm là:", "Đại lượng vô hướng", new[]{ "Đại lượng vectơ", "Luôn dương", "Luôn âm" }),
                ("Đơn vị điện dung:", "Fara (F)", new[]{ "Vôn (V)", "Ohm (Ω)", "Henry (H)" }),
                ("Mạch điện nối tiếp: R_td = ?", "R₁ + R₂ + ... + Rₙ", new[]{ "1/R₁ + 1/R₂", "R₁·R₂/(R₁+R₂)", "R₁ - R₂" }),
                ("Mạch điện song song: 1/R_td = ?", "1/R₁ + 1/R₂", new[]{ "R₁ + R₂", "R₁·R₂", "R₁/R₂" }),
                ("Năng lượng tụ điện: W = ?", "Q²/(2C)", new[]{ "Q·C/2", "C/(2Q²)", "Q/C" }),
                ("Chiết suất n = ?", "c/v", new[]{ "v/c", "c·v", "c+v" }),
                ("Ánh sáng truyền từ nước ra không khí:", "Có thể xảy ra phản xạ toàn phần", new[]{ "Không thể phản xạ toàn phần", "Luôn khúc xạ", "Không truyền được" }),
                ("Từ trường của dòng điện thẳng dài:", "Các đường tròn đồng tâm", new[]{ "Đường thẳng song song", "Đường xoắn ốc", "Đường thẳng vuông góc" }),
                ("Suất điện động cảm ứng: e = ?", "-dΦ/dt", new[]{ "dΦ/dt", "Φ/t", "t/Φ" }),
                ("Hệ số tự cảm đơn vị:", "Henry (H)", new[]{ "Fara (F)", "Weber (Wb)", "Tesla (T)" }),
                ("Định luật Kirchhoff 1 (nút):", "ΣI_vào = ΣI_ra", new[]{ "ΣU = 0", "ΣR = const", "I = U/R" }),
                ("Điện trở phụ thuộc:", "Vật liệu, chiều dài, tiết diện", new[]{ "Điện áp đặt vào", "Cường độ dòng điện", "Công suất" }),
            },

            // ── Vật lý 12 (30 câu) ────────────────────────────────────────
            [3] = new[]
            {
                ("Dao động điều hoà có phương trình:", "x = A·cos(ωt + φ)", new[]{ "x = A·sin(ωt)", "x = A·ωt", "x = A/(ωt)" }),
                ("Chu kỳ dao động T = ?", "2π/ω", new[]{ "ω/2π", "2πω", "π/ω" }),
                ("Tần số f = ?", "1/T", new[]{ "T", "2πT", "T/2π" }),
                ("Con lắc lò xo: ω = ?", "√(k/m)", new[]{ "√(m/k)", "k/m", "m/k" }),
                ("Con lắc đơn: T = ?", "2π√(l/g)", new[]{ "2π√(g/l)", "2π√l", "π√(l/g)" }),
                ("Sóng cơ truyền được trong:", "Môi trường vật chất", new[]{ "Chân không", "Mọi môi trường", "Chỉ trong chất rắn" }),
                ("Bước sóng λ = ?", "v·T = v/f", new[]{ "v/T", "f·T", "T/v" }),
                ("Giao thoa sóng xảy ra khi:", "Hai sóng kết hợp gặp nhau", new[]{ "Bất kỳ hai sóng nào", "Sóng phản xạ", "Sóng nhiễu xạ" }),
                ("Sóng dừng có:", "Nút và bụng sóng cố định", new[]{ "Nút di chuyển", "Không có nút", "Bụng di chuyển" }),
                ("Mạch RLC cộng hưởng khi:", "ω²LC = 1", new[]{ "ωL = R", "ωC = R", "L = C" }),
                ("Công suất điện xoay chiều: P = ?", "U·I·cos(φ)", new[]{ "U·I", "U·I·sin(φ)", "U²/R" }),
                ("Máy biến áp: U₁/U₂ = ?", "N₁/N₂", new[]{ "N₂/N₁", "I₁/I₂", "I₂/I₁" }),
                ("Hạt nhân nguyên tử gồm:", "Proton và neutron", new[]{ "Proton và electron", "Neutron và electron", "Chỉ proton" }),
                ("Phóng xạ α phát ra:", "Hạt nhân ⁴He", new[]{ "Electron", "Positron", "Photon" }),
                ("Phóng xạ β⁻ phát ra:", "Electron (e⁻)", new[]{ "Positron", "Hạt α", "Neutron" }),
                ("Năng lượng liên kết hạt nhân:", "Năng lượng để tách hạt nhân thành các nuclon", new[]{ "Năng lượng phóng xạ", "Năng lượng nhiệt hạch", "Năng lượng kết hợp proton" }),
                ("Hiện tượng quang điện xảy ra khi:", "f ánh sáng ≥ f giới hạn", new[]{ "f ánh sáng < f giới hạn", "Cường độ đủ lớn", "Ánh sáng nhìn thấy" }),
                ("Công thức Einstein quang điện: eV₀ = ?", "hf - A", new[]{ "hf + A", "A - hf", "hf·A" }),
                ("Tia X có bản chất là:", "Sóng điện từ bước sóng ngắn", new[]{ "Hạt α", "Hạt β", "Sóng âm" }),
                ("Laser là ánh sáng:", "Kết hợp, đơn sắc, định hướng cao", new[]{ "Trắng, mạnh", "Nhiều màu, mạnh", "Đơn sắc, phân kỳ cao" }),
                ("Cơ năng con lắc lò xo: W = ?", "½kA²", new[]{ "½mv²", "kx²", "mgh" }),
                ("Độ lệch pha giữa u và i trong mạch thuần R:", "0", new[]{ "π/2", "π", "-π/2" }),
                ("Tổng trở mạch RLC nối tiếp: Z = ?", "√(R² + (Lω - 1/Cω)²)", new[]{ "R + Lω + 1/Cω", "√(Lω - 1/Cω)", "R + Lω" }),
                ("Chu kỳ bán rã T₁/₂ là:", "Thời gian để N giảm còn N/2", new[]{ "Thời gian phân rã hết", "Thời gian sống trung bình", "Hằng số phóng xạ" }),
                ("Phản ứng phân hạch dùng trong:", "Nhà máy điện hạt nhân", new[]{ "Bom nhiệt hạch", "Pin mặt trời", "Tia laser" }),
                ("Tốc độ ánh sáng trong chân không:", "3×10⁸ m/s", new[]{ "3×10⁶ m/s", "3×10¹⁰ m/s", "3×10⁴ m/s" }),
                ("Hiệu ứng Doppler âm thanh xảy ra khi:", "Nguồn hoặc máy thu chuyển động", new[]{ "Sóng phản xạ", "Sóng dừng", "Sóng nhiễu xạ" }),
                ("Mức cường độ âm L = ?", "10·log(I/I₀) dB", new[]{ "log(I/I₀) dB", "I/I₀ dB", "100·log(I/I₀) dB" }),
                ("Điện từ trường lan truyền với tốc độ:", "Tốc độ ánh sáng", new[]{ "Tốc độ âm", "Tốc độ điện tử", "Tốc độ sóng cơ" }),
                ("Máy phát điện xoay chiều hoạt động dựa trên:", "Cảm ứng điện từ", new[]{ "Quang điện", "Nhiệt điện", "Điện phân" }),
            },

            // ── Hóa 10 (30 câu) ────────────────────────────────────────────
            [4] = new[]
            {
                ("Nguyên tử gồm:", "Hạt nhân và vỏ electron", new[]{ "Chỉ electron", "Chỉ proton và neutron", "Proton, neutron, electron đều ở hạt nhân" }),
                ("Số hiệu nguyên tử Z là:", "Số proton", new[]{ "Số neutron", "Số electron ngoài cùng", "Số khối" }),
                ("Số khối A = ?", "Z + N", new[]{ "Z - N", "Z × N", "Z / N" }),
                ("Đồng vị là các nguyên tử có cùng:", "Số proton, khác số neutron", new[]{ "Số neutron, khác proton", "Số khối", "Số electron ngoài cùng" }),
                ("Cấu hình electron của Na (Z=11):", "[Ne]3s¹", new[]{ "[Ne]3s²", "1s²2s²2p⁶3s²", "1s²2s²2p⁵" }),
                ("Bảng tuần hoàn sắp xếp các nguyên tố theo:", "Số hiệu nguyên tử tăng dần", new[]{ "Nguyên tử khối tăng dần", "Nhóm", "Chu kỳ" }),
                ("Độ âm điện tăng theo chiều:", "Từ trái sang phải trong cùng chu kỳ", new[]{ "Từ phải sang trái", "Từ trên xuống trong cùng nhóm", "Từ dưới lên trong cùng nhóm" }),
                ("Liên kết ion hình thành giữa:", "Kim loại mạnh và phi kim mạnh", new[]{ "Hai phi kim", "Hai kim loại", "Phi kim và phi kim yếu" }),
                ("Liên kết cộng hóa trị là liên kết do:", "Dùng chung electron", new[]{ "Cho nhận electron", "Lực hút tĩnh điện", "Lực Van der Waals" }),
                ("Phản ứng oxi hóa - khử là phản ứng có:", "Sự thay đổi số oxi hóa", new[]{ "Kết tủa tạo thành", "Sự thay đổi màu sắc", "Nhiệt lượng tỏa ra" }),
                ("Chất oxi hóa là chất:", "Nhận electron", new[]{ "Nhường electron", "Không thay đổi số oxi hóa", "Tạo kết tủa" }),
                ("Số oxi hóa của O trong H₂O:", "-2", new[]{ "+2", "0", "-1" }),
                ("Số oxi hóa của H trong HCl:", "+1", new[]{ "-1", "0", "+2" }),
                ("Kim loại kiềm thuộc nhóm:", "IA", new[]{ "IIA", "VIIA", "IB" }),
                ("Phi kim mạnh nhất là:", "Flo (F)", new[]{ "Clo (Cl)", "Oxy (O)", "Nitơ (N)" }),
                ("Axit là chất:", "Cho proton (H⁺)", new[]{ "Nhận proton", "Cho electron", "Nhận electron" }),
                ("Bazơ là chất:", "Nhận proton (H⁺)", new[]{ "Cho proton", "Cho electron", "Nhận electron" }),
                ("pH của dung dịch trung tính ở 25°C:", "7", new[]{ "0", "14", "1" }),
                ("Phản ứng trao đổi ion xảy ra khi tạo:", "Kết tủa, khí hoặc nước", new[]{ "Muối mới", "Axit mới", "Bazơ mới" }),
                ("Nguyên tắc bảo toàn electron:", "Tổng e nhường = Tổng e nhận", new[]{ "e nhường > e nhận", "e nhận > e nhường", "Không cần bảo toàn" }),
                ("Clo (Cl₂) có màu:", "Vàng lục", new[]{ "Không màu", "Đỏ nâu", "Tím" }),
                ("HCl là:", "Axit mạnh", new[]{ "Axit yếu", "Bazơ", "Muối" }),
                ("NaOH là:", "Bazơ mạnh", new[]{ "Bazơ yếu", "Axit", "Muối" }),
                ("Phản ứng trung hòa: HCl + NaOH →", "NaCl + H₂O", new[]{ "NaCl + H₂", "NaH + HClO", "Na + HCl + OH" }),
                ("Kim loại nào phản ứng với nước ở nhiệt độ thường?", "Na", new[]{ "Fe", "Cu", "Al" }),
                ("Oxit axit khi tác dụng với nước tạo:", "Axit", new[]{ "Bazơ", "Muối", "Oxit mới" }),
                ("Oxit bazơ khi tác dụng với nước tạo:", "Bazơ", new[]{ "Axit", "Muối", "Oxit mới" }),
                ("Tốc độ phản ứng tăng khi:", "Tăng nồng độ chất tham gia", new[]{ "Giảm nhiệt độ", "Giảm áp suất", "Giảm diện tích tiếp xúc" }),
                ("Xúc tác là chất:", "Làm tăng tốc độ phản ứng, không bị tiêu thụ", new[]{ "Bị tiêu thụ trong phản ứng", "Làm giảm tốc độ", "Tạo sản phẩm mới" }),
                ("Số oxi hóa của Mn trong KMnO₄:", "+7", new[]{ "+4", "+2", "+6" }),
            },

            // ── Hóa 12 (30 câu) ────────────────────────────────────────────
            [5] = new[]
            {
                ("Este no, đơn chức có công thức:", "CₙH₂ₙO₂ (n≥2)", new[]{ "CₙH₂ₙO (n≥1)", "CₙHₙO₂", "CₙH₂ₙ₋₂O₂" }),
                ("Phản ứng xà phòng hóa là phản ứng:", "Thủy phân este trong kiềm", new[]{ "Este + axit", "Este + nước", "Axit + kiềm" }),
                ("Glucozơ có công thức phân tử:", "C₆H₁₂O₆", new[]{ "C₁₂H₂₂O₁₁", "C₆H₁₀O₅", "C₂H₅OH" }),
                ("Glucozơ tham gia phản ứng tráng gương vì:", "Có nhóm CHO", new[]{ "Có nhóm OH", "Có nhóm COOH", "Có nhóm C=O dạng ceton" }),
                ("Saccarozơ thủy phân tạo:", "Glucozơ + Fructozơ", new[]{ "Chỉ glucozơ", "Chỉ fructozơ", "Glucozơ + Galactozơ" }),
                ("Tinh bột cho phản ứng màu xanh với:", "Iot (I₂)", new[]{ "Nước brom", "NaOH", "HCl" }),
                ("Amin bậc 1 có công thức:", "RNH₂", new[]{ "R₂NH", "R₃N", "R₄N⁺" }),
                ("Aminoaxit vừa tác dụng được với:", "Axit và bazơ (lưỡng tính)", new[]{ "Chỉ axit", "Chỉ bazơ", "Không tác dụng axit-bazơ" }),
                ("Liên kết peptit là liên kết:", "-CO-NH-", new[]{ "-O-", "-S-S-", "-NH₄⁺-" }),
                ("Protein bị thủy phân tạo:", "Aminoaxit", new[]{ "Glucozơ", "Axit béo", "Glycerol" }),
                ("Kim loại có tính dẫn điện tốt nhất:", "Ag (Bạc)", new[]{ "Cu (Đồng)", "Au (Vàng)", "Al (Nhôm)" }),
                ("Kim loại kiềm có hóa trị:", "I (+1)", new[]{ "II (+2)", "III (+3)", "IV (+4)" }),
                ("Al phản ứng được với:", "Axit, bazơ và oxit kim loại", new[]{ "Chỉ axit", "Chỉ bazơ", "Chỉ muối" }),
                ("NaOH + Al + H₂O tạo:", "NaAlO₂ + H₂", new[]{ "Al₂O₃ + NaH", "AlOH + Na", "AlNa + H₂O" }),
                ("Sắt có các hóa trị:", "+2 và +3", new[]{ "Chỉ +2", "Chỉ +3", "+1 và +2" }),
                ("Fe₂O₃ là oxit của Fe hóa trị:", "+3", new[]{ "+2", "+1", "+4" }),
                ("Thép là hợp kim của:", "Fe và C (0.01% - 2% C)", new[]{ "Fe và Cu", "Fe và Ni", "Fe và C (>2% C)" }),
                ("Gang là hợp kim Fe-C với C:", ">2%", new[]{ "<0.01%", "0.01%-2%", "=2%" }),
                ("Polietilen (PE) được điều chế từ:", "Etilen (CH₂=CH₂)", new[]{ "Axetilen", "Metan", "Propilen" }),
                ("Cao su thiên nhiên là polime của:", "Isopren", new[]{ "Butadien", "Vinyl clorua", "Styren" }),
                ("Phản ứng este hóa: RCOOH + R'OH →", "RCOOR' + H₂O", new[]{ "RCOOR' + H₂", "RCO + R'OH₂", "RCOOH₂ + R'O" }),
                ("Chất béo là este của:", "Glycerol và axit béo", new[]{ "Glucozơ và axit béo", "Glycerol và axit hữu cơ bất kỳ", "Aminoaxit và axit béo" }),
                ("Sữa chua hình thành do:", "Lên men lactic của đường sữa (lactozơ)", new[]{ "Lên men rượu", "Thủy phân protein", "Oxi hóa glucozơ" }),
                ("Cu không phản ứng với:", "HCl loãng", new[]{ "HNO₃ loãng", "H₂SO₄ đặc nóng", "FeCl₃" }),
                ("Phản ứng trùng hợp là:", "Nhiều monome → polime, không tạo sản phẩm phụ", new[]{ "Nhiều monome → polime + H₂O", "Polime → monome", "Monome + dung môi → polime" }),
                ("Glucozơ lên men rượu tạo:", "C₂H₅OH + CO₂", new[]{ "CH₃OH + CO₂", "C₂H₅OH + H₂O", "CH₃COOH + H₂" }),
                ("Nước cứng là nước chứa nhiều ion:", "Ca²⁺ và Mg²⁺", new[]{ "Na⁺ và K⁺", "Fe²⁺ và Fe³⁺", "Cl⁻ và SO₄²⁻" }),
                ("Xà phòng là muối:", "Natri hoặc kali của axit béo", new[]{ "Natri của HCl", "Canxi của axit béo", "Este của axit béo" }),
                ("Tơ tằm là:", "Protein thiên nhiên", new[]{ "Polisaccarit", "Este tổng hợp", "Poliamit tổng hợp" }),
                ("Nylon-6,6 là polime của:", "Hexametylendiamin + axit adipic", new[]{ "Caprolactam", "Etilen + vinyl clorua", "Acrylonitril" }),
            },

            // ── Tiếng Anh 10 (30 câu) ─────────────────────────────────────
            [6] = new[]
            {
                ("Choose the correct tense: She ___ (study) English every day.", "studies", new[]{ "is studying", "studied", "has studied" }),
                ("Choose the correct tense: They ___ (play) football now.", "are playing", new[]{ "play", "played", "have played" }),
                ("Choose the correct form: I ___ (not see) him since Monday.", "haven't seen", new[]{ "didn't see", "don't see", "wasn't seeing" }),
                ("Choose the correct word: She is ___ than her sister.", "taller", new[]{ "more tall", "tallest", "tall" }),
                ("Choose the correct word: He is ___ student in the class.", "the most intelligent", new[]{ "more intelligent", "intelligent", "intelligenter" }),
                ("Choose the correct preposition: She is good ___ math.", "at", new[]{ "in", "on", "with" }),
                ("Choose the correct preposition: He arrived ___ time.", "on", new[]{ "in", "at", "by" }),
                ("Choose the correct article: ___ sun rises in the east.", "The", new[]{ "A", "An", "No article" }),
                ("Choose the correct pronoun: ___ is my book.", "This", new[]{ "These", "Those", "That are" }),
                ("Choose the correct word: They ___ to school yesterday.", "went", new[]{ "go", "goes", "going" }),
                ("Choose the odd one out: cat, dog, bird, table", "table", new[]{ "cat", "dog", "bird" }),
                ("Choose the correct word: The weather is ___ today. (sunny/sun/sunlight/sunshine)", "sunny", new[]{ "sun", "sunlight", "sunshine" }),
                ("Choose the correct sentence:", "She doesn't like coffee.", new[]{ "She don't like coffee.", "She not like coffee.", "She isn't like coffee." }),
                ("'Enormous' means:", "Very large", new[]{ "Very small", "Very fast", "Very slow" }),
                ("Opposite of 'ancient':", "Modern", new[]{ "Old", "Antique", "Traditional" }),
                ("Choose the correct passive: 'They build the house.'", "The house is built by them.", new[]{ "The house built by them.", "The house is build by them.", "The house was built by them." }),
                ("Choose the correct conditional: If I ___ rich, I would travel.", "were", new[]{ "am", "will be", "would be" }),
                ("Choose the correct relative pronoun: The girl ___ won the prize is my friend.", "who", new[]{ "which", "that", "whom" }),
                ("Choose the correct word: She asked me ___ I had finished.", "whether", new[]{ "that", "what", "which" }),
                ("'Environment' means:", "The natural world around us", new[]{ "The weather", "The city", "The school" }),
                ("Choose the correct conjunction: I was tired, ___ I went to bed early.", "so", new[]{ "but", "because", "although" }),
                ("Choose the correct word: He studies very ___.", "hard", new[]{ "hardly", "hardness", "harder" }),
                ("'Benefit' means:", "Advantage", new[]{ "Disadvantage", "Problem", "Danger" }),
                ("Choose the correct question: ___ does she live?", "Where", new[]{ "What", "Who", "How" }),
                ("Choose the correct word: The book ___ on the shelf.", "is", new[]{ "are", "am", "were" }),
                ("'Technology' is a ___ noun.", "Uncountable", new[]{ "Countable", "Proper", "Abstract" }),
                ("Choose the correct sentence:", "He has been living here for 5 years.", new[]{ "He is living here for 5 years.", "He lived here for 5 years.", "He live here for 5 years." }),
                ("'Generous' means:", "Willing to give freely", new[]{ "Selfish", "Greedy", "Mean" }),
                ("Choose the synonym of 'important':", "Significant", new[]{ "Trivial", "Minor", "Unimportant" }),
                ("Choose the correct sentence:", "The more you practice, the better you get.", new[]{ "The more you practice, the more you get better.", "More you practice, better you get.", "The more practice, the more better." }),
            },

            // ── Tiếng Anh luyện thi (30 câu) ──────────────────────────────
            [7] = new[]
            {
                ("Choose the word whose underlined part is pronounced differently: A. change B. charge C. machine D. chain", "machine", new[]{ "change", "charge", "chain" }),
                ("Choose the correct form: The meeting was ___ due to bad weather.", "cancelled", new[]{ "cancelling", "cancel", "cancels" }),
                ("Choose the word that best fits: She showed great ___ in finishing the project.", "determination", new[]{ "determinate", "determine", "determined" }),
                ("'Bilingual' means:", "Able to speak two languages", new[]{ "Speaking one language", "Learning a language", "Translating languages" }),
                ("Choose the correct sentence:", "Not only did he win, but he also broke the record.", new[]{ "Not only he won but also he broke the record.", "Not only he did win but he broke the record.", "He not only won but also he broke record." }),
                ("Choose the synonym of 'PRESERVE':", "Conserve", new[]{ "Destroy", "Abandon", "Neglect" }),
                ("Choose the antonym of 'PESSIMISTIC':", "Optimistic", new[]{ "Realistic", "Negative", "Depressed" }),
                ("Choose the correct relative clause: The city ___ I was born is beautiful.", "where", new[]{ "which", "that", "who" }),
                ("Choose the correct reported speech: 'I will help you.' He said that he ___.", "would help me", new[]{ "will help me", "would help you", "will help you" }),
                ("Choose the correct passive: 'People speak English worldwide.'", "English is spoken worldwide.", new[]{ "English speaks worldwide.", "English was spoken worldwide.", "English is speaking worldwide." }),
                ("Choose the word closest in meaning to 'ABUNDANT':", "Plentiful", new[]{ "Scarce", "Limited", "Rare" }),
                ("Choose the correct conjunction: ___ it rained heavily, they continued playing.", "Although", new[]{ "Because", "So", "Therefore" }),
                ("Choose the correct form: I wish I ___ more time to study.", "had", new[]{ "have", "will have", "would have" }),
                ("Choose the correct idiom: 'Break a leg' means:", "Good luck", new[]{ "Get injured", "Run fast", "Stop trying" }),
                ("Choose the correct collocation: make a ___", "decision", new[]{ "travel", "work", "go" }),
                ("Choose the correct word form: His speech was very ___. (IMPRESS)", "impressive", new[]{ "impression", "impress", "impressed" }),
                ("Choose the sentence with correct grammar:", "Had I known, I would have helped.", new[]{ "If I had knew, I would help.", "If I knew, I would have helped.", "Had I know, I would help." }),
                ("Choose the sentence closest in meaning: 'It's possible that she forgot.'", "She might have forgotten.", new[]{ "She should have forgotten.", "She must have forgotten.", "She couldn't have forgotten." }),
                ("'Deforestation' means:", "Clearing forests", new[]{ "Planting trees", "Forest fire", "Forest research" }),
                ("Choose the correct preposition: She is responsible ___ the project.", "for", new[]{ "of", "to", "with" }),
                ("Choose the correct connector: I studied hard; ___, I failed the exam.", "however", new[]{ "therefore", "moreover", "furthermore" }),
                ("'Curriculum' means:", "Course of study", new[]{ "School building", "Teacher training", "Exam schedule" }),
                ("Choose the correct form: The results will be ___ next week.", "announced", new[]{ "announcing", "announce", "announces" }),
                ("Choose the correct word: Scientists have ___ a new vaccine.", "developed", new[]{ "discovering", "invent", "creating" }),
                ("'Sustainable development' means:", "Development that meets present needs without harming future generations", new[]{ "Fast economic growth", "Industrial development", "Urban expansion" }),
                ("Choose the correct form: She's been working here ___ 2018.", "since", new[]{ "for", "during", "from" }),
                ("Choose the correct sentence:", "The younger the children are, the faster they learn languages.", new[]{ "The more young the children, the more fast they learn.", "Younger children learn faster languages.", "The youngest children learn the fastest languages." }),
                ("'Bilingualism' refers to:", "The ability to use two languages", new[]{ "Using one language only", "Learning a second language", "Translating between languages" }),
                ("Choose the correct modal: You ___ drive if you're tired.", "shouldn't", new[]{ "mustn't", "don't have to", "can't" }),
                ("Choose the word most opposite to 'COMPULSORY':", "Optional", new[]{ "Necessary", "Mandatory", "Required" }),
            },

            // ── Ngữ văn 11 (30 câu) ────────────────────────────────────────
            [8] = new[]
            {
                ("Tác giả của 'Tự tình II' là:", "Hồ Xuân Hương", new[]{ "Nguyễn Du", "Nguyễn Khuyến", "Trần Tế Xương" }),
                ("'Câu cá mùa thu' thuộc thể thơ:", "Thất ngôn bát cú Đường luật", new[]{ "Lục bát", "Song thất lục bát", "Tứ tuyệt" }),
                ("Tác giả 'Thương vợ' là:", "Trần Tế Xương", new[]{ "Hồ Xuân Hương", "Nguyễn Khuyến", "Tú Xương (khác)" }),
                ("'Chí Phèo' của Nam Cao thuộc thể loại:", "Truyện ngắn", new[]{ "Tiểu thuyết", "Thơ", "Ký" }),
                ("Nhân vật Chí Phèo là đại diện cho:", "Người nông dân bị xã hội tha hóa", new[]{ "Giai cấp địa chủ", "Tri thức nghèo", "Người buôn bán" }),
                ("'Hai đứa trẻ' của Thạch Lam thuộc xu hướng:", "Văn học lãng mạn", new[]{ "Văn học hiện thực", "Văn học cách mạng", "Văn học dân gian" }),
                ("Bức tranh phố huyện trong 'Hai đứa trẻ' được miêu tả vào:", "Lúc chiều tà đến đêm", new[]{ "Buổi sáng sớm", "Buổi trưa", "Đêm khuya" }),
                ("'Chữ người tử tù' của Nguyễn Tuân đề cao:", "Cái đẹp và thiên lương", new[]{ "Sức mạnh quyền lực", "Tình yêu đôi lứa", "Tinh thần yêu nước" }),
                ("Nhân vật Huấn Cao trong 'Chữ người tử tù' là:", "Người cho chữ - nghệ sĩ tài hoa", new[]{ "Quan cai ngục", "Thầy thơ lại", "Người tù thường" }),
                ("Biện pháp tu từ trong câu 'Lom khom dưới núi tiều vài chú':", "Đảo ngữ", new[]{ "So sánh", "Nhân hóa", "Ẩn dụ" }),
                ("'Vội vàng' của Xuân Diệu thể hiện:", "Khát vọng sống mãnh liệt và nỗi sợ thời gian", new[]{ "Tình yêu quê hương", "Tinh thần cách mạng", "Nỗi nhớ người thân" }),
                ("'Đây thôn Vĩ Dạ' của Hàn Mặc Tử có mấy khổ thơ?", "3 khổ", new[]{ "4 khổ", "2 khổ", "5 khổ" }),
                ("Tràng giang của Huy Cận mang cảm hứng từ:", "Sông Hồng và tứ thơ Đường", new[]{ "Sông Hương", "Sông Cửu Long", "Biển Đông" }),
                ("'Chiều tối' là bài thơ của Hồ Chí Minh sáng tác:", "Trên đường bị giải đi qua các nhà lao Quảng Tây", new[]{ "Ở chiến khu Việt Bắc", "Ở Hà Nội", "Ở Pháp" }),
                ("Phong cách nghệ thuật của Nguyễn Tuân:", "Tài hoa, uyên bác, độc đáo", new[]{ "Giản dị, mộc mạc", "Hài hước, dí dỏm", "Lãng mạn, bay bổng" }),
                ("'Vào phủ chúa Trịnh' thuộc thể loại:", "Ký sự", new[]{ "Truyện ngắn", "Tiểu thuyết", "Thơ" }),
                ("Tác phẩm nào của Nguyễn Khuyến thuộc chùm thơ thu?", "Thu điếu, Thu ẩm, Thu vịnh", new[]{ "Thương vợ, Tự trào", "Qua đèo Ngang, Chiều hôm nhớ nhà", "Đoạn trường tân thanh" }),
                ("Nội dung chính của 'Thương vợ' (Trần Tế Xương):", "Hình ảnh người vợ tần tảo và tình cảm của tác giả", new[]{ "Phê phán xã hội phong kiến", "Ca ngợi người phụ nữ anh hùng", "Nỗi nhớ quê hương" }),
                ("Nghĩa luận văn học cần có những phần chính:", "Mở bài, thân bài, kết bài", new[]{ "Luận điểm, luận cứ", "Giới thiệu, phân tích, đánh giá", "Dẫn chứng, bình luận" }),
                ("'Đường vào phủ chúa Trịnh' được viết theo:", "Ngôi thứ nhất - tác giả trực tiếp kể", new[]{ "Ngôi thứ ba", "Ngôi thứ hai", "Không xác định ngôi kể" }),
                ("Chủ đề bao quát của văn học lãng mạn 1930-1945:", "Cái tôi cá nhân, thoát ly thực tại", new[]{ "Đấu tranh giai cấp", "Yêu nước cách mạng", "Phản ánh hiện thực xã hội" }),
                ("Từ 'xanh xao' trong câu thơ Hồ Xuân Hương là biện pháp:", "Tính từ gợi tả trạng thái", new[]{ "Ẩn dụ", "Hoán dụ", "Nhân hóa" }),
                ("'Câu cá mùa thu' phản ánh tâm trạng:", "Cô đơn, u buồn, trăn trở về thế sự", new[]{ "Vui vẻ, lạc quan", "Tức giận, phản kháng", "Yêu đời, sôi nổi" }),
                ("Phong trào Thơ Mới 1932-1945 đặc trưng bởi:", "Đề cao cái tôi cá nhân, phá vỡ niêm luật cũ", new[]{ "Tuân theo niêm luật Đường thi", "Nội dung cách mạng, chống giặc", "Thơ văn xuôi" }),
                ("Nam Cao sinh năm:", "1917", new[]{ "1920", "1915", "1910" }),
                ("'Chí Phèo' ban đầu có tên:", "Cái lò gạch cũ", new[]{ "Chí Phèo", "Đôi lứa xứng đôi", "Người hàng xóm" }),
                ("Thị Nở trong 'Chí Phèo' là hình ảnh:", "Tình người, tia sáng nhân tính", new[]{ "Ác nhân", "Nạn nhân của Chí Phèo", "Người hoàn toàn xấu xa" }),
                ("Biện pháp nghệ thuật đặc sắc trong 'Hai đứa trẻ':", "Tương phản sáng - tối", new[]{ "Điệp ngữ", "Phép đối", "Liệt kê" }),
                ("Thể thơ của 'Vội vàng' (Xuân Diệu):", "Tự do (có xen câu ngắn)", new[]{ "Thất ngôn", "Lục bát", "Song thất lục bát" }),
                ("Tên thật của Hồ Xuân Hương theo một số tài liệu:", "Không rõ tên thật", new[]{ "Nguyễn Thị Hương", "Hồ Phi Mai", "Nguyễn Hương" }),
            },

            // ── Ngữ văn 12 (30 câu) ────────────────────────────────────────
            [9] = new[]
            {
                ("'Tây Tiến' của Quang Dũng viết về:", "Đoàn quân Tây Tiến và vẻ đẹp miền Tây Bắc", new[]{ "Chiến trường miền Nam", "Biển đảo Việt Nam", "Kháng chiến chống Pháp ở đồng bằng" }),
                ("'Việt Bắc' của Tố Hữu thuộc thể thơ:", "Lục bát", new[]{ "Thất ngôn", "Tự do", "Song thất lục bát" }),
                ("'Đất Nước' (trích Mặt đường khát vọng) của:", "Nguyễn Khoa Điềm", new[]{ "Tố Hữu", "Chế Lan Viên", "Phạm Tiến Duật" }),
                ("'Sóng' của Xuân Quỳnh là bài thơ về:", "Tình yêu đôi lứa và khát vọng hạnh phúc", new[]{ "Cách mạng và kháng chiến", "Thiên nhiên đất nước", "Tình mẫu tử" }),
                ("'Chiếc thuyền ngoài xa' thuộc thể loại:", "Truyện ngắn", new[]{ "Tiểu thuyết", "Ký", "Thơ" }),
                ("Tác giả 'Chiếc thuyền ngoài xa' là:", "Nguyễn Minh Châu", new[]{ "Nguyễn Khải", "Ma Văn Kháng", "Tô Hoài" }),
                ("Nhân vật Phùng trong 'Chiếc thuyền ngoài xa' là:", "Nhiếp ảnh gia", new[]{ "Ngư dân", "Nhà văn", "Nhà giáo" }),
                ("Thông điệp của 'Chiếc thuyền ngoài xa':", "Nghệ thuật phải gắn với cuộc đời và sự thật", new[]{ "Phê phán chiến tranh", "Ca ngợi vẻ đẹp thiên nhiên", "Tình yêu lứa đôi" }),
                ("'Tây Tiến' sử dụng bút pháp:", "Lãng mạn và bi tráng", new[]{ "Hiện thực phê phán", "Hiện thực xã hội chủ nghĩa", "Tượng trưng, siêu thực" }),
                ("'Đất Nước' - Nguyễn Khoa Điềm định nghĩa Đất Nước qua:", "Lịch sử, văn hóa và nhân dân", new[]{ "Địa lý tự nhiên", "Chiến tranh cách mạng", "Kinh tế xã hội" }),
                ("Hình tượng 'sóng' trong thơ Xuân Quỳnh là:", "Ẩn dụ cho tình yêu của người phụ nữ", new[]{ "Tả thực sóng biển", "Biểu tượng cách mạng", "Hình ảnh đất nước" }),
                ("'Việt Bắc' - Tố Hữu được viết nhân sự kiện:", "Trung ương Đảng và Chính phủ rời Việt Bắc về Hà Nội (1954)", new[]{ "Chiến thắng Điện Biên Phủ", "Hiệp định Genève", "Ngày giải phóng miền Nam" }),
                ("Câu hỏi tu từ trong 'Sóng': 'Ôi con sóng ngày xưa / Và ngày sau vẫn thế' thể hiện:", "Tình yêu vĩnh cửu, bất biến", new[]{ "Nỗi nhớ nhung", "Sự thất vọng", "Niềm vui" }),
                ("Phương thức biểu đạt chính của 'Chiếc thuyền ngoài xa':", "Tự sự", new[]{ "Biểu cảm", "Miêu tả", "Nghị luận" }),
                ("Trong đọc hiểu, biện pháp 'điệp ngữ' có tác dụng:", "Nhấn mạnh, tạo nhịp điệu", new[]{ "Làm rõ đặc điểm", "Tạo hình ảnh cụ thể", "Gợi liên tưởng" }),
                ("'Ẩn dụ' là biện pháp:", "Gọi tên sự vật, hiện tượng này bằng tên sự vật hiện tượng khác có nét tương đồng", new[]{ "Gọi tên bằng tên sự vật có quan hệ gần gũi", "Phóng đại mức độ", "Nhân hóa sự vật" }),
                ("Đoạn văn nghị luận xã hội 200 chữ cần có:", "Giới thiệu vấn đề, giải thích, bình luận, liên hệ", new[]{ "Chỉ cần kể chuyện", "Chỉ cần dẫn chứng", "Liệt kê số liệu" }),
                ("Chủ nghĩa nhân đạo trong văn học Việt Nam thể hiện:", "Trân trọng phẩm giá con người, thương cảm số phận", new[]{ "Ca ngợi tự nhiên", "Phê phán kẻ thù", "Tôn vinh anh hùng" }),
                ("'Rừng xà nu' của Nguyễn Trung Thành viết về:", "Cuộc kháng chiến của dân làng Xô Man", new[]{ "Vẻ đẹp Tây Nguyên", "Chiến trường miền Bắc", "Hậu phương miền Nam" }),
                ("Cấu trúc bài nghị luận văn học hoàn chỉnh:", "MB (giới thiệu) - TB (phân tích, chứng minh) - KB (đánh giá, mở rộng)", new[]{ "Chỉ cần thân bài", "MB - TB không cần KB", "Liệt kê dẫn chứng" }),
                ("Tố Hữu là nhà thơ tiêu biểu của:", "Thơ cách mạng và kháng chiến", new[]{ "Thơ lãng mạn 1930-1945", "Thơ hiện đại hậu chiến", "Thơ dân gian" }),
                ("Quang Dũng sinh năm:", "1921", new[]{ "1915", "1930", "1925" }),
                ("Hình ảnh 'chiếc xe không kính' trong thơ Phạm Tiến Duật tượng trưng:", "Tinh thần lạc quan, dũng cảm của người lính", new[]{ "Sự nghèo nàn của quân đội", "Chiến tranh tàn phá", "Nỗi nhớ quê hương" }),
                ("'Người lái đò sông Đà' của Nguyễn Tuân thuộc thể loại:", "Tùy bút", new[]{ "Truyện ngắn", "Tiểu thuyết", "Ký sự" }),
                ("Phong cách nghệ thuật Xuân Quỳnh:", "Chân thực, dịu dàng, nhiều lo âu khắc khoải", new[]{ "Hào hùng, mạnh mẽ", "Hài hước, trào phúng", "Siêu thực, tượng trưng" }),
                ("'Ai đã đặt tên cho dòng sông?' của:", "Hoàng Phủ Ngọc Tường", new[]{ "Nguyễn Tuân", "Nguyễn Khải", "Tô Hoài" }),
                ("Đặc trưng của ký là:", "Người thật, việc thật; tác giả trực tiếp tham gia hoặc chứng kiến", new[]{ "Hư cấu hoàn toàn", "Nhân vật điển hình", "Cốt truyện li kỳ" }),
                ("'Vợ nhặt' của Kim Lân bối cảnh:", "Nạn đói 1945", new[]{ "Kháng chiến chống Pháp", "Kháng chiến chống Mỹ", "Thời kỳ đổi mới" }),
                ("Nhân vật Tràng trong 'Vợ nhặt' đại diện cho:", "Người nghèo khổ với khát vọng sống và yêu thương", new[]{ "Địa chủ bóc lột", "Trí thức thành thị", "Người lính cách mạng" }),
                ("Hình thức nghị luận 'bình luận' khác 'phân tích' ở chỗ:", "Có thể đồng ý hoặc phản bác, đưa ra quan điểm riêng", new[]{ "Chỉ giải thích", "Chỉ mô tả", "Không cần dẫn chứng" }),
            },
        };

        // ── Tạo Questions + MultipleChoiceAnswers cho từng môn ───────────

        // Lưu câu hỏi theo index môn để dùng cho ExamPaperDetail
        var questionsBySubject = new Dictionary<int, List<Question>>();
        var allMcAnswers       = new List<MultipleChoiceAnswer>();

        for (int subjectIdx = 0; subjectIdx < 10; subjectIdx++)
        {
            // Mỗi đề dùng chung ngân hàng 30 câu của môn đó (index 0-4 dùng bank[0..4], 5-9 dùng bank[5..9])
            int bankKey       = subjectIdx % 10; // vì có đúng 10 bank
            var bank          = questionBanks[bankKey < questionBanks.Count ? bankKey : bankKey % questionBanks.Count];
            var subjectQuestions = new List<Question>();

            foreach (var (q, correct, wrongs) in bank)
            {
                var question = new Question
                {
                    Id             = Guid.NewGuid(),
                    Title          = q,
                    TypeOfQuestion = QuestionType.MultipleChoice,
                    Point          = Math.Round(10m / bank.Length, 2), // 10đ / 30 câu ≈ 0.33đ
                };
                subjectQuestions.Add(question);

                allMcAnswers.Add(new MultipleChoiceAnswer { Id = Guid.NewGuid(), QuestionId = question.Id, Content = correct, IsCorrect = true });
                foreach (var w in wrongs)
                    allMcAnswers.Add(new MultipleChoiceAnswer { Id = Guid.NewGuid(), QuestionId = question.Id, Content = w, IsCorrect = false });
            }

            questionsBySubject[subjectIdx] = subjectQuestions;
            await context.Questions.AddRangeAsync(subjectQuestions);
        }

        await context.MultipleChoiceAnswers.AddRangeAsync(allMcAnswers);

        // ── ExamPaper + Deadline (1 bộ / môn) ────────────────────────────

        var examConfigs = new[]
        {
            ("Kiểm tra giữa kỳ Toán 10",         lecturers[0].Id, 0,  50, ExamPaperStatus.Open,   7),
            ("Kiểm tra cuối kỳ Toán 12",          lecturers[0].Id, 1,  90, ExamPaperStatus.Open,   14),
            ("Kiểm tra 15 phút Vật lý 11",        lecturers[1].Id, 2,  45, ExamPaperStatus.Open,   3),
            ("Đề thi học kỳ Vật lý 12",           lecturers[1].Id, 3,  90, ExamPaperStatus.Closed, -1),
            ("Kiểm tra chương 1 Hóa 10",          lecturers[2].Id, 4,  45, ExamPaperStatus.Open,   5),
            ("Đề thi cuối kỳ Hóa 12",             lecturers[2].Id, 5,  90, ExamPaperStatus.Open,   10),
            ("Kiểm tra từ vựng Tiếng Anh 10",     lecturers[3].Id, 6,  30, ExamPaperStatus.Open,   2),
            ("Đề thi thử THPT Tiếng Anh",         lecturers[3].Id, 7,  60, ExamPaperStatus.Open,   7),
            ("Kiểm tra Ngữ văn 11",               lecturers[4].Id, 8,  60, ExamPaperStatus.Open,   5),
            ("Đề thi học kỳ Ngữ văn 12",          lecturers[4].Id, 9, 90, ExamPaperStatus.Open,   10),
        };

        var examPapers    = new List<ExamPaper>();
        var deadlines     = new List<Deadline>();
        var paperDetails  = new List<ExamPaperDetail>();

        // Map lesson đầu tiên của mỗi course
        var lessonByCourse = lessons
            .GroupBy(l => l.CourseId)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.Position).First());

        for (int i = 0; i < examConfigs.Length; i++)
        {
            var (title, lecId, subjectIdx, countdown, status, endDays) = examConfigs[i];
            var examId     = Guid.NewGuid();
            var deadlineId = Guid.NewGuid();

            var courseLesson = lessonByCourse.ContainsKey(courses[i].Id)
                ? lessonByCourse[courses[i].Id]
                : lessons.First();

            var exam = new ExamPaper
            {
                Id          = examId,
                LecturerId  = lecId,
                LessonId    = courseLesson.Id,
                DeadlineId  = deadlineId,
                Title       = title,
                CountDown   = countdown,
                TotalPoints = 10.0m,
                Status      = status,
                CreatedAt   = now,
            };

            var deadline = new Deadline
            {
                Id          = deadlineId,
                ExamPaperId = examId,
                Title       = $"Hạn nộp: {title}",
                EndedAt     = now.AddDays(endDays),
                Status      = endDays < 0 ? DeadlineStatus.Completed : DeadlineStatus.Processing,
                CreatedAt   = now,
            };

            examPapers.Add(exam);
            deadlines.Add(deadline);

            // Gán toàn bộ 30 câu của môn vào chi tiết đề
            foreach (var q in questionsBySubject[subjectIdx])
                paperDetails.Add(new ExamPaperDetail { Id = Guid.NewGuid(), ExamPaperId = examId, QuestionId = q.Id });
        }

        await context.ExamPapers.AddRangeAsync(examPapers);
        await context.Deadlines.AddRangeAsync(deadlines);
        await context.ExamPaperDetails.AddRangeAsync(paperDetails);

        // ── ExamManagement + Detail (5 student đã mua nộp bài) ────────────

        var examManagements   = new List<ExamManament>();
        var managementDetails = new List<ExamManementDetail>();

        for (int si = 0; si < 5; si++)
        {
            var student      = students[5 + si];
            var targetExam   = examPapers[si * 2];
            var isGoodScore  = si % 2 == 0;
            var detailsForExam = paperDetails.Where(pd => pd.ExamPaperId == targetExam.Id).ToList();
            var subjectIdx   = si * 2;

            var management = new ExamManament
            {
                Id              = Guid.NewGuid(),
                ExamPaperId     = targetExam.Id,
                StudentId       = student.Id,
                PointsOfStudent = isGoodScore ? 8.67m : 5.0m,
            };
            examManagements.Add(management);

            foreach (var detail in detailsForExam)
            {
                var question    = questionsBySubject[subjectIdx].First(q => q.Id == detail.QuestionId);
                var correctAns  = allMcAnswers.First(a => a.QuestionId == question.Id && a.IsCorrect);
                var wrongAns    = allMcAnswers.First(a => a.QuestionId == question.Id && !a.IsCorrect);

                managementDetails.Add(new ExamManementDetail
                {
                    Id                     = Guid.NewGuid(),
                    ExamManementId         = management.Id,
                    ExamPaperDetailId      = detail.Id,
                    IsMultiChoice          = true,
                    MultipleChoiceAnswerId = isGoodScore ? correctAns.Id : wrongAns.Id,
                    Answer                 = "",
                    Point                  = isGoodScore ? question.Point : 0m,
                    Feedback               = isGoodScore ? "Chính xác!" : "Sai. Xem lại lý thuyết.",
                });
            }
        }

        await context.ExamManagements.AddRangeAsync(examManagements);
        await context.ExamManagementDetails.AddRangeAsync(managementDetails);

        // ── ExamComment ───────────────────────────────────────────────────

        var examComments = new List<ExamComment>();
        var parentContents = new[]
        {
            "Đề hay, bám sát SGK và cấu trúc thi thật!",
            "Câu hỏi số 15 hơi mơ hồ, mong thầy/cô xem lại.",
            "Thời gian làm bài vừa đủ, không quá áp lực.",
            "Barem chấm điểm chi tiết và rõ ràng.",
            "Mong ra thêm câu vận dụng cao cho kỳ sau ạ.",
        };
        var replyContents = new[]
        {
            "Mình cũng thấy vậy, rất sát đề thi thật 2024!",
            "Đồng ý, câu đó mình cũng nhầm đáp án.",
            "Ừ nhỉ, thi xong còn dư 5 phút để kiểm tra.",
            "Cảm ơn thầy/cô đã đầu tư soạn barem kỹ ạ!",
            "Hy vọng năm sau đề khó hơn để thách thức hơn.",
        };

        for (int i = 0; i < 5; i++)
        {
            var parent = new ExamComment
            {
                Id                  = Guid.NewGuid(),
                ExamPaperId         = examPapers[i].Id,
                ParentExamCommentId = null,
                Content             = parentContents[i],
                CreatedAt           = now.AddMinutes(-60 + i * 10),
                NumberOfLikes       = (i + 1) * 4,
                NumberOfDislikes    = i,
            };
            examComments.Add(parent);

            examComments.Add(new ExamComment
            {
                Id                  = Guid.NewGuid(),
                ExamPaperId         = examPapers[i].Id,
                ParentExamCommentId = parent.Id,
                Content             = replyContents[i],
                CreatedAt           = now.AddMinutes(-30 + i * 5),
                NumberOfLikes       = i + 1,
                NumberOfDislikes    = 0,
            });
        }

        await context.ExamComments.AddRangeAsync(examComments);
        await context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 5. LEARNING PROCESS — đủ toàn bộ lesson đã enroll
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task SeedLearningProcessAsync(AppDbContext context)
    {
        if (await context.LearningProcesses.AnyAsync()) return;

        var now = DateTimeOffset.UtcNow;

        var enrollments = await context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .ToListAsync();

        var learningProcesses = new List<LearningProcess>();

        foreach (var enrollment in enrollments)
        {
            var orderedLessons = enrollment.Course.Lessons
                .OrderBy(l => l.Position)
                .ToList();

            int totalLessons = orderedLessons.Count;

            for (int i = 0; i < totalLessons; i++)
            {
                // Hoàn thành 70% bài học, 30% còn lại đang xem dở
                bool isCompleted = i < (int)(totalLessons * 0.7);
                int watchTime    = isCompleted
                    ? orderedLessons[i].Duration * 60       // xem hết (giây)
                    : (orderedLessons[i].Duration * 60 / 2); // xem được nửa

                learningProcesses.Add(new LearningProcess
                {
                    Id            = Guid.NewGuid(),
                    StuId         = enrollment.StuId,
                    LessonId      = orderedLessons[i].Id,
                    WatchTime     = watchTime,
                    IsCompleted   = isCompleted,
                    LastWatchedAt = now.AddDays(-(totalLessons - i)), // xem theo thứ tự từ cũ đến mới
                });
            }
        }

        await context.LearningProcesses.AddRangeAsync(learningProcesses);
        await context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // 6. DOCUMENTS
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task SeedDocumentsAsync(AppDbContext context)
    {
        if (await context.Documents.AnyAsync()) return;

        var lessons = await context.Lessons
            .Where(l => l.IsPreview)
            .Take(20)
            .ToListAsync();

        var documents = lessons.SelectMany(lesson => new[]
        {
            new Document
            {
                Id       = Guid.NewGuid(),
                LessonId = lesson.Id,
                FileName = $"Slide_{lesson.Title[..Math.Min(lesson.Title.Length, 20)]}.pptx",
                FileUrl  = $"https://storage.smartcenter.vn/docs/slide_{lesson.Id}.pptx",
                FileType = "PPTX",
            },
            new Document
            {
                Id       = Guid.NewGuid(),
                LessonId = lesson.Id,
                FileName = $"BaiTap_{lesson.Title[..Math.Min(lesson.Title.Length, 20)]}.pdf",
                FileUrl  = $"https://storage.smartcenter.vn/docs/baitap_{lesson.Id}.pdf",
                FileType = "PDF",
            },
        }).ToList();

        await context.Documents.AddRangeAsync(documents);
        await context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static User MakeUser(string firstName, string lastName,
        string email, string phone, UserRole role, string password, DateTimeOffset now) => new()
    {
        Id           = Guid.NewGuid(),
        FirstName    = firstName,
        LastName     = lastName,
        Email        = email,
        Phone        = phone,
        Role         = role,
        Status       = UserStatus.Active,
        Verified     = true,
        VerifiedCode = 0,
        PasswordHash = HashPassword(password),
        CreatedAt    = now,
    };

    private static Course MakeCourse(Guid lecId, string name, string description,
        decimal price, CourseType type, int maxStudents, DateTimeOffset now) => new()
    {
        Id           = Guid.NewGuid(),
        LecId        = lecId,
        CourseName   = name,
        Description  = description,
        BasePrice    = price,
        ImgUrl       = $"https://placehold.co/600x400?text={Uri.EscapeDataString(name[..Math.Min(name.Length, 10)])}",
        CourseType   = type,
        IsActive     = true,
        StartAt      = now,
        EndAt        = now.AddMonths(4),
        MaxStudents  = maxStudents,
        AcademicYear = 2025,
        CreatedAt    = now,
    };
}