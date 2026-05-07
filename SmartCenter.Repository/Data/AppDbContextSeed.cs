using Microsoft.EntityFrameworkCore;
using SmartCenter.Repository.Entity;
using SmartCenter.Repository.Entity.Enums;
using Transaction = SmartCenter.Repository.Entity.Transaction;
using System.Security.Cryptography;
using System.Text;

namespace SmartCenter.Repository.Data;

public static class AppDbContextSeed
{
    private static readonly string VideoUrl = "https://youtu.be/vZJbfYUvbQM?si=nFBTdZXAfxKIrR_7";

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public static async Task SeedAsync(AppDbContext context)
    {
        // if (await context.Users.AnyAsync()) return;

        var now = DateTimeOffset.UtcNow;

        // ADMIN

        var adminUser = MakeUser("SmartCenter", "Admin", "admin@smartcenter.vn", "0961002445", UserRole.Admin,
            "Admin@123", now);

        await context.Users.AddAsync(adminUser);

        // LECTURERS

        var lecUserData = new[]
        {
            ("Trần", "Thị Minh Châu", "minhchau@smartcenter.vn", "0912345001", "Toán học"),
            ("Lê", "Văn Hùng", "vanhung@smartcenter.vn", "0912345002", "Vật lý"),
            ("Phạm", "Thị Thu Hương", "thuhuong@smartcenter.vn", "0912345003", "Hóa học"),
            ("Võ", "Quang Minh", "quangminh@smartcenter.vn", "0912345004", "Tiếng Anh"),
            ("Nguyễn", "Thị Lan Anh", "lananh@smartcenter.vn", "0912345005", "Ngữ văn"),
        };

        var lecUsers = lecUserData.Select(d =>
            MakeUser(d.Item1, d.Item2, d.Item3, d.Item4, UserRole.Lecturer, "Lecturer@123", now)
        ).ToList();

        await context.Users.AddRangeAsync(lecUsers);

        var lecturers = lecUsers.Zip(lecUserData, (u, d) => new Lecturer
        {
            Id = Guid.NewGuid(),
            UserId = u.Id,
            Expertise = d.Item5,
            Bio = $"Giảng viên {d.Item5} với nhiều năm kinh nghiệm giảng dạy cấp THPT.",
            CreatedAt = now,
        }).ToList();

        await context.Lecturers.AddRangeAsync(lecturers);

        // STUDENTS

        var stuUserData = new[]
        {
            ("Hoàng", "Minh Tuấn", "minhtuan@gmail.com", "0923456001"),
            ("Nguyễn", "Thị Thùy Linh", "thuylinh@gmail.com", "0923456002"),
            ("Trần", "Văn Khánh", "vankhanh@gmail.com", "0923456003"),
            ("Lê", "Thị Ngọc Hân", "ngochan@gmail.com", "0923456004"),
            ("Phạm", "Đức Vinh", "ducvinh@gmail.com", "0923456005"),
            ("Bùi", "Thị Thảo Nhi", "thaonhi@gmail.com", "0923456006"),
            ("Võ", "Thành Liêm", "thanhliem@gmail.com", "0923456007"),
            ("Đặng", "Thị Mỹ Duyên", "myduyen@gmail.com", "0923456008"),
            ("Nguyễn", "Hoàng Lộc", "hoangloc@gmail.com", "0923456009"),
            ("Trịnh", "Thị Kim Ánh", "kimanh@gmail.com", "0923456010"),
        };

        var stuUsers = stuUserData.Select(d =>
            MakeUser(d.Item1, d.Item2, d.Item3, d.Item4, UserRole.Student, "Student@123", now)
        ).ToList();

        await context.Users.AddRangeAsync(stuUsers);

        // Tạo Cart trước vì Student.CartId là FK
        var carts = stuUsers.Select(_ => new Cart { Id = Guid.NewGuid(), StuId = Guid.Empty }).ToList();
        await context.Carts.AddRangeAsync(carts);

        var students = stuUsers.Select((u, i) =>
        {
            var student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = u.Id,
                CartId = carts[i].Id,
                Address = $"Số {(i + 1) * 10} Đường Lê Lợi, Quận {i % 5 + 1}",
                City = i % 2 == 0 ? "TP. Hồ Chí Minh" : "Hà Nội",
                EnrollmentDate = now,
                CreatedAt = now,
            };
            carts[i].StuId = student.Id;
            return student;
        }).ToList();

        await context.Students.AddRangeAsync(students);

        // COURSES

        var courses = new List<Course>
        {
            MakeCourse(lecturers[0].Id, "Toán 10 — Đại số và Giải tích",
                "Nắm vững chương trình Toán lớp 10: hàm số, phương trình, bất phương trình, lượng giác.",
                350000, CourseType.Online, 50, now),

            MakeCourse(lecturers[0].Id, "Toán 12 — Luyện thi THPT Quốc gia",
                "Ôn tập toàn bộ chương trình Toán 12, giải đề thi thử và phân tích đề thi thật.",
                450000, CourseType.Online, 60, now),

            MakeCourse(lecturers[1].Id, "Vật lý 11 — Điện học & Quang học",
                "Chương trình Vật lý lớp 11: điện tích, điện trường, dòng điện, khúc xạ ánh sáng.",
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
                "Phân tích các tác phẩm văn học Việt Nam lớp 11, kỹ năng viết nghị luận văn học.",
                250000, CourseType.Online, 60, now),

            MakeCourse(lecturers[4].Id, "Ngữ văn 12 — Luyện thi THPT Quốc gia",
                "Ôn tập đọc hiểu, nghị luận xã hội, nghị luận văn học theo cấu trúc đề thi mới.",
                350000, CourseType.Online, 70, now),
        };

        await context.Courses.AddRangeAsync(courses);

        // SECTIONS + LESSONS

        var allSections = new List<Section>();
        var allLessons = new List<Lesson>();

        void AddContent(Course course, (string SecTitle, string[] LessonTitles)[] data)
        {
            for (int si = 0; si < data.Length; si++)
            {
                var sec = new Section
                {
                    Id = Guid.NewGuid(),
                    CourseId = course.Id,
                    Title = data[si].SecTitle,
                    Position = si + 1,
                    IsActive = true,
                };
                allSections.Add(sec);

                for (int li = 0; li < data[si].LessonTitles.Length; li++)
                {
                    allLessons.Add(new Lesson
                    {
                        Id = Guid.NewGuid(),
                        SectionId = sec.Id,
                        CourseId = course.Id,
                        Title = data[si].LessonTitles[li],
                        Description = $"Nội dung bài: {data[si].LessonTitles[li]}",
                        VideoUrl = VideoUrl,
                        Duration = 30 + (li * 5),
                        Position = li + 1,
                        IsPreview = li == 0, // bài đầu mỗi section là preview
                        CreatedAt = now,
                    });
                }
            }
        }

        // ── Toán 10 
        AddContent(courses[0], new[]
        {
            ("Mệnh đề & Tập hợp",
                new[] { "Mệnh đề và các phép toán", "Tập hợp và các phép toán tập hợp", "Số gần đúng và sai số" }),
            ("Hàm số bậc nhất & bậc hai",
                new[] { "Hàm số và đồ thị", "Hàm số bậc nhất", "Hàm số bậc hai", "Vẽ đồ thị parabol" }),
            ("Phương trình & Hệ phương trình",
                new[]
                {
                    "Phương trình bậc nhất, bậc hai", "Hệ phương trình bậc nhất hai ẩn", "Phương trình quy về bậc hai"
                }),
            ("Bất phương trình",
                new[] { "Bất đẳng thức", "Bất phương trình bậc nhất", "Dấu nhị thức và tam thức bậc hai" }),
            ("Lượng giác",
                new[]
                {
                    "Cung và góc lượng giác", "Giá trị lượng giác", "Công thức lượng giác",
                    "Phương trình lượng giác cơ bản"
                }),
        });

        // ── Toán 12 luyện thi 
        AddContent(courses[1], new[]
        {
            ("Ứng dụng đạo hàm",
                new[] { "Tính đơn điệu của hàm số", "Cực trị của hàm số", "Giá trị lớn nhất - nhỏ nhất", "Tiệm cận" }),
            ("Hàm số mũ & Logarit",
                new[]
                {
                    "Hàm số mũ và đồ thị", "Hàm số logarit", "Phương trình mũ - logarit",
                    "Bất phương trình mũ - logarit"
                }),
            ("Nguyên hàm & Tích phân",
                new[]
                {
                    "Nguyên hàm cơ bản", "Tích phân xác định", "Tính diện tích bằng tích phân",
                    "Tính thể tích bằng tích phân"
                }),
            ("Số phức",
                new[] { "Khái niệm số phức", "Các phép toán số phức", "Phương trình bậc hai nghiệm phức" }),
            ("Luyện đề tổng hợp",
                new[]
                {
                    "Phân tích cấu trúc đề thi", "Luyện đề thử số 1", "Luyện đề thử số 2", "Giải đề thi thật 2024"
                }),
        });

        // ── Vật lý 11
        AddContent(courses[2], new[]
        {
            ("Điện tích & Điện trường",
                new[]
                {
                    "Điện tích - Định luật Coulomb", "Điện trường", "Công của lực điện", "Điện thế và hiệu điện thế"
                }),
            ("Tụ điện & Dòng điện",
                new[] { "Tụ điện", "Dòng điện không đổi", "Nguồn điện - Định luật Ohm", "Định luật Kirchhoff" }),
            ("Từ trường",
                new[] { "Từ trường và đường sức từ", "Lực từ - Lực Lorentz", "Cảm ứng điện từ", "Tự cảm" }),
            ("Quang hình học",
                new[] { "Khúc xạ ánh sáng", "Phản xạ toàn phần", "Lăng kính", "Thấu kính" }),
        });

        // ── Vật lý 12 luyện thi
        AddContent(courses[3], new[]
        {
            ("Dao động cơ",
                new[] { "Dao động điều hoà", "Con lắc lò xo", "Con lắc đơn", "Tổng hợp dao động" }),
            ("Sóng cơ & Sóng âm",
                new[] { "Sóng cơ học", "Giao thoa sóng", "Sóng dừng", "Đặc trưng vật lý sóng âm" }),
            ("Điện xoay chiều",
                new[] { "Mạch RLC nối tiếp", "Cộng hưởng điện", "Công suất điện xoay chiều", "Máy biến áp" }),
            ("Hạt nhân nguyên tử",
                new[] { "Cấu tạo hạt nhân", "Phóng xạ", "Phản ứng hạt nhân", "Năng lượng hạt nhân" }),
            ("Luyện đề tổng hợp",
                new[] { "Chiến lược làm bài thi", "Đề thử số 1", "Đề thử số 2", "Giải đề thi thật 2024" }),
        });

        // ── Hóa 10 
        AddContent(courses[4], new[]
        {
            ("Nguyên tử",
                new[] { "Thành phần nguyên tử", "Hạt nhân và vỏ electron", "Cấu hình electron", "Đồng vị" }),
            ("Bảng tuần hoàn",
                new[] { "Cấu tạo bảng tuần hoàn", "Xu hướng biến đổi tuần hoàn", "Ý nghĩa bảng tuần hoàn" }),
            ("Liên kết hóa học",
                new[] { "Liên kết ion", "Liên kết cộng hóa trị", "Liên kết kim loại", "Hiệu độ âm điện" }),
            ("Phản ứng oxi hóa - khử",
                new[]
                {
                    "Số oxi hóa", "Phản ứng oxi hóa - khử", "Cân bằng phản ứng theo electron", "Ứng dụng thực tiễn"
                }),
        });

        // ── Hóa 12 luyện thi 
        AddContent(courses[5], new[]
        {
            ("Este & Lipit",
                new[] { "Khái niệm và tính chất este", "Phản ứng xà phòng hóa", "Chất béo", "Bài toán hỗn hợp este" }),
            ("Cacbohiđrat",
                new[] { "Glucozơ", "Saccarozơ", "Tinh bột và xenlulozơ", "Bài toán tổng hợp" }),
            ("Amin - Aminoaxit - Protein",
                new[] { "Amin và tính chất", "Aminoaxit và muối amoni", "Peptit và protein", "Bài tập tổng hợp" }),
            ("Kim loại",
                new[]
                {
                    "Tính chất chung kim loại", "Kim loại kiềm và kiềm thổ", "Nhôm và hợp chất", "Sắt và hợp chất"
                }),
            ("Luyện đề tổng hợp",
                new[] { "Phân tích đề và chiến lược", "Đề thử số 1", "Đề thử số 2", "Giải đề thi thật 2024" }),
        });

        // ── Tiếng Anh 10 ─
        AddContent(courses[6], new[]
        {
            ("Ngữ pháp nền tảng",
                new[]
                {
                    "Thì hiện tại đơn & tiếp diễn", "Thì quá khứ đơn & tiếp diễn", "Thì tương lai", "Câu điều kiện"
                }),
            ("Từ vựng theo chủ đề",
                new[] { "Family & Relationships", "Education & School", "Environment", "Technology" }),
            ("Kỹ năng Reading",
                new[] { "Đọc hiểu điền từ", "Đọc hiểu trả lời câu hỏi", "Chiến lược skimming & scanning" }),
            ("Kỹ năng Writing",
                new[] { "Viết đoạn văn cơ bản", "Viết thư", "Viết bài luận ngắn" }),
        });

        // ── Tiếng Anh luyện thi
        AddContent(courses[7], new[]
        {
            ("Ngữ pháp trọng tâm",
                new[] { "Mệnh đề quan hệ", "Câu bị động", "Câu gián tiếp", "Từ nối và liên từ" }),
            ("Từ vựng thi THPT",
                new[] { "Từ đồng nghĩa - trái nghĩa", "Thành ngữ thông dụng", "Word form", "Collocations" }),
            ("Kỹ năng làm bài thi",
                new[]
                {
                    "Phần Pronunciation", "Phần Grammar & Vocabulary", "Phần Reading comprehension", "Phần Writing"
                }),
            ("Luyện đề",
                new[]
                {
                    "Đề thử số 1 có giải", "Đề thử số 2 có giải", "Phân tích lỗi sai thường gặp",
                    "Giải đề thi thật 2024"
                }),
        });

        // ── Ngữ văn 11 
        AddContent(courses[8], new[]
        {
            ("Văn học trung đại Việt Nam",
                new[]
                {
                    "Vào phủ chúa Trịnh - Lê Hữu Trác", "Tự tình II - Hồ Xuân Hương", "Câu cá mùa thu - Nguyễn Khuyến",
                    "Thương vợ - Trần Tế Xương"
                }),
            ("Thơ lãng mạn 1930-1945",
                new[]
                {
                    "Vội vàng - Xuân Diệu", "Đây thôn Vĩ Dạ - Hàn Mặc Tử", "Tràng giang - Huy Cận",
                    "Chiều tối - Hồ Chí Minh"
                }),
            ("Văn xuôi hiện đại",
                new[] { "Hai đứa trẻ - Thạch Lam", "Chữ người tử tù - Nguyễn Tuân", "Chí Phèo - Nam Cao" }),
            ("Kỹ năng viết nghị luận",
                new[]
                {
                    "Nghị luận về tư tưởng đạo lý", "Nghị luận về hiện tượng xã hội", "Nghị luận về một đoạn thơ",
                    "Nghị luận về tác phẩm văn xuôi"
                }),
        });

        // ── Ngữ văn 12 luyện thi
        AddContent(courses[9], new[]
        {
            ("Đọc hiểu",
                new[]
                {
                    "Phương thức biểu đạt", "Biện pháp tu từ", "Nội dung và ý nghĩa văn bản",
                    "Chiến lược làm phần đọc hiểu"
                }),
            ("Nghị luận xã hội",
                new[]
                {
                    "Cách viết đoạn văn 200 chữ", "Dạng tư tưởng đạo lý", "Dạng hiện tượng xã hội",
                    "Luyện viết có nhận xét"
                }),
            ("Nghị luận văn học",
                new[]
                {
                    "Tây Tiến - Quang Dũng", "Việt Bắc - Tố Hữu", "Đất Nước - Nguyễn Khoa Điềm", "Sóng - Xuân Quỳnh",
                    "Chiếc thuyền ngoài xa - Nguyễn Minh Châu"
                }),
            ("Luyện đề tổng hợp",
                new[] { "Phân tích cấu trúc đề thi", "Đề thử số 1", "Đề thử số 2", "Giải đề thi thật 2024" }),
        });

        await context.Sections.AddRangeAsync(allSections);
        await context.Lessons.AddRangeAsync(allLessons);


        // CART ITEMS (student 1–5 thêm vào giỏ, chưa mua)

        var cartItems = new List<CartItem>
        {
            new()
            {
                Id = Guid.NewGuid(), CartId = carts[0].Id, CourseId = courses[0].Id, ItemType = CartItemType.Course,
                Quantity = 1, CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CartId = carts[0].Id, CourseId = courses[2].Id, ItemType = CartItemType.Course,
                Quantity = 1, CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CartId = carts[1].Id, CourseId = courses[1].Id, ItemType = CartItemType.Course,
                Quantity = 1, CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CartId = carts[2].Id, CourseId = courses[4].Id, ItemType = CartItemType.Course,
                Quantity = 1, CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CartId = carts[3].Id, CourseId = courses[6].Id, ItemType = CartItemType.Course,
                Quantity = 1, CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CartId = carts[3].Id, CourseId = courses[7].Id, ItemType = CartItemType.Course,
                Quantity = 1, CreatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CartId = carts[4].Id, CourseId = courses[8].Id, ItemType = CartItemType.Course,
                Quantity = 1, CreatedAt = now
            },
        };
        await context.CartItems.AddRangeAsync(cartItems);


        // ORDERS + ORDER ITEMS + ENROLLMENTS (student 6–10 đã mua)


        var purchasePlan = new[]
        {
            (students[5], new[] { courses[0], courses[1] }), // Toán 10 + Toán 12
            (students[6], new[] { courses[2], courses[3] }), // Vật lý 11 + Vật lý 12
            (students[7], new[] { courses[4], courses[5] }), // Hóa 10 + Hóa 12
            (students[8], new[] { courses[6], courses[7] }), // Anh 10 + Anh luyện thi
            (students[9], new[] { courses[8], courses[9] }), // Văn 11 + Văn 12
        };

        var orderCodeBase = now.Ticks;
        foreach (var (student, boughtCourses) in purchasePlan)
        {
            var subtotal = boughtCourses.Sum(c => c.BasePrice);
            var order = new Order
            {
                Id = Guid.NewGuid(),
                StuId = student.Id,
                OrderCode = $"ORD{orderCodeBase++}",
                Status = OrderStatus.Paid,
                PaymentMethod = PaymentMethod.BankTransfer,
                SubtotalAmount = subtotal,
                DiscountAmount = 0,
                TotalAmount = subtotal,
                CreatedAt = now,
                UpdatedAt = now,
                ExpireAt = now.AddMinutes(15),
                PaidAt = now,
            };
            await context.Orders.AddAsync(order);

            // Tạo Transaction cho Order này
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Amount = subtotal,
                Status = "Full Complete",
                ProviderTransactionCode = $"TXN{orderCodeBase}",
                ConfirmedByStaffId = adminUser.Id,
                ConfirmedAt = now,
                CreatedAt = now,
            };
            await context.Transactions.AddAsync(transaction);

            foreach (var course in boughtCourses)
            {
                await context.OrderItems.AddAsync(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    CourseId = course.Id,
                    ItemName = course.CourseName,
                    UnitPrice = course.BasePrice,
                    Quantity = 1,
                    CreatedAt = now,
                });

                await context.Enrollments.AddAsync(new Enrollment
                {
                    Id = Guid.NewGuid(),
                    CourseId = course.Id,
                    StuId = student.Id,
                    Status = EnrollmentStatus.Paid,
                    EnrollmentDate = now,
                    TransactionId = transaction.Id,
                    CreatedAt = now,
                });
            }
        }

        await context.SaveChangesAsync();

        // 1. NGÂN HÀNG CÂU HỎI (QUESTIONS) & ĐÁP ÁN (ANSWERS)
        // Tạo 10 câu hỏi: 6 Trắc nghiệm, 4 Tự luận đa dạng thể loại. Lưu ý: Điểm dùng hậu tố 'm' (decimal)
        var questionsData = new[]
        {
            (Title: "Giải phương trình bậc hai: x^2 - 5x + 6 = 0", Type: QuestionType.MultipleChoice, Point: 2.0m),
            (Title: "Đạo hàm của hàm số y = sin(x) là gì?", Type: QuestionType.MultipleChoice, Point: 2.0m),
            (Title: "Thủ đô của Việt Nam là gì?", Type: QuestionType.MultipleChoice, Point: 1.0m),
            (Title: "Trong C#, từ khóa 'virtual' dùng để làm gì?", Type: QuestionType.MultipleChoice, Point: 2.5m),
            (Title: "Đơn vị đo cường độ dòng điện là?", Type: QuestionType.MultipleChoice, Point: 1.5m),
            (Title: "Tác giả của Truyện Kiều là ai?", Type: QuestionType.MultipleChoice, Point: 1.0m),

            (Title: "Phân tích tâm lý nhân vật Mị trong đêm tình mùa xuân (Vợ Chồng A Phủ).", Type: QuestionType.Essay,
                Point: 8.0m),
            (Title: "Trình bày nguyên lý hoạt động của Garbage Collector trong .NET.", Type: QuestionType.Essay,
                Point: 7.5m),
            (Title: "Viết một đoạn văn tiếng Anh ngắn (150 chữ) về lợi ích của AI.", Type: QuestionType.Essay,
                Point: 8.5m),
            (Title: "Chứng minh định lý Pythagoras bằng hình học.", Type: QuestionType.Essay, Point: 9.0m)
        };

        var questions = questionsData.Select(q => new Question
        {
            Id = Guid.NewGuid(), Title = q.Title, TypeOfQuestion = q.Type, Point = q.Point
        }).ToList();
        await context.Questions.AddRangeAsync(questions);

        // Tạo đáp án cho 6 câu trắc nghiệm (Mỗi câu 4 đáp án: 1 Đúng, 3 Sai)
        var mcAnswers = new List<MultipleChoiceAnswer>();
        var esAnswers = new List<EssayAnswer>();

        for (int i = 0; i < 6; i++)
        {
            mcAnswers.Add(new MultipleChoiceAnswer
            {
                Id = Guid.NewGuid(), QuestionId = questions[i].Id, Content = $"Đáp án đúng của câu {i + 1}",
                IsCorrect = true
            });
            mcAnswers.Add(new MultipleChoiceAnswer
            {
                Id = Guid.NewGuid(), QuestionId = questions[i].Id, Content = $"Đáp án sai A của câu {i + 1}",
                IsCorrect = false
            });
            mcAnswers.Add(new MultipleChoiceAnswer
            {
                Id = Guid.NewGuid(), QuestionId = questions[i].Id, Content = $"Đáp án sai B của câu {i + 1}",
                IsCorrect = false
            });
            mcAnswers.Add(new MultipleChoiceAnswer
            {
                Id = Guid.NewGuid(), QuestionId = questions[i].Id, Content = $"Đáp án sai C của câu {i + 1}",
                IsCorrect = false
            });
        }

        await context.MultipleChoiceAnswers.AddRangeAsync(mcAnswers);

        for (int i = 6; i < 10; i++)
        {
            esAnswers.Add(new EssayAnswer
            {
                Id = Guid.NewGuid(), QuestionId = questions[i].Id,
                Content = $"Barem chấm điểm chuẩn cho câu tự luận {i + 1}..."
            });
        }

        await context.EssayAnswers.AddRangeAsync(esAnswers);


        // 2. THỜI HẠN (DEADLINE) & ĐỀ THI (EXAMPAPER) - TẠO 5 BỘ
        var examPapers = new List<ExamPaper>();
        var deadlines = new List<Deadline>();
        var paperDetails = new List<ExamPaperDetail>();

        var examConfigs = new[]
        {
            (Title: "Đề thi giữa kỳ Toán", CountDown: 60, EStatus: ExamPaperStatus.Open,
                DStatus: DeadlineStatus.Processing, EndDays: 7),
            (Title: "Đề thi cuối kỳ Lập Trình", CountDown: 120, EStatus: ExamPaperStatus.Closed,
                DStatus: DeadlineStatus.Processing, EndDays: -2), // Đã quá hạn
            (Title: "Bài test nhanh Tiếng Anh", CountDown: 15, EStatus: ExamPaperStatus.Open,
                DStatus: DeadlineStatus.Processing, EndDays: 1),
            (Title: "Đề thi nháp (Đã hủy)", CountDown: 45, EStatus: ExamPaperStatus.Deleted,
                DStatus: DeadlineStatus.Processing, EndDays: 0),
            (Title: "Kiểm tra 15 phút Vật Lý", CountDown: 15, EStatus: ExamPaperStatus.Open,
                DStatus: DeadlineStatus.Processing, EndDays: 3)
        };

        for (int i = 0; i < examConfigs.Length; i++)
        {
            var examId = Guid.NewGuid();
            var deadlineId = Guid.NewGuid();

            var exam = new ExamPaper
            {
                Id = examId,
                LecturerId = lecturers[0].Id,
                DeadlineId = deadlineId,

                // THÊM DÒNG NÀY ĐỂ FIX LỖI KHÓA NGOẠI LESSON_ID:
                LessonId = allLessons[i].Id,

                Title = examConfigs[i].Title,
                CountDown = examConfigs[i].CountDown,
                TotalPoints = 10.0m,
                Status = examConfigs[i].EStatus
            };

            var deadline = new Deadline
            {
                Id = deadlineId, ExamPaperId = examId, Title = $"Hạn nộp: {examConfigs[i].Title}",
                CreatedAt = now, EndedAt = now.AddDays(examConfigs[i].EndDays), Status = examConfigs[i].DStatus
            };

            examPapers.Add(exam);
            deadlines.Add(deadline);

            // 3. CHI TIẾT ĐỀ THI (Mỗi đề lấy ngẫu nhiên 2 câu hỏi làm ví dụ)
            paperDetails.Add(new ExamPaperDetail
                { Id = Guid.NewGuid(), ExamPaperId = exam.Id, QuestionId = questions[i * 2].Id });
            paperDetails.Add(new ExamPaperDetail
                { Id = Guid.NewGuid(), ExamPaperId = exam.Id, QuestionId = questions[i == 4 ? 0 : (i * 2) + 1].Id });
        }

        await context.ExamPapers.AddRangeAsync(examPapers);
        await context.Deadlines.AddRangeAsync(deadlines);
        await context.ExamPaperDetails.AddRangeAsync(paperDetails);


        // 4 & 5. QUẢN LÝ BÀI LÀM (EXAM_MANAGEMENT) VÀ CHI TIẾT LÀM BÀI - TẠO 10 LƯỢT
        var examManagements = new List<ExamManament>();
        var managementDetails = new List<ExamManementDetail>();

        for (int i = 0; i < 10; i++)
        {
            var targetExam = examPapers[i % 5];
            var currentStudent = students[i];
            var examDetailsForThisPaper = paperDetails.Where(pd => pd.ExamPaperId == targetExam.Id).ToList();

            var management = new ExamManament
            {
                Id = Guid.NewGuid(), ExamPaperId = targetExam.Id, StudentId = currentStudent.Id,
                PointsOfStudent = (i % 2 == 0) ? 8.5m : 4.0m // Sửa thành 8.5m và 4.0m
            };
            examManagements.Add(management);

            foreach (var detail in examDetailsForThisPaper)
            {
                var question = questions.First(q => q.Id == detail.QuestionId);
                var isMCQ = question.TypeOfQuestion == QuestionType.MultipleChoice;

                var submissionDetail = new ExamManementDetail
                {
                    Id = Guid.NewGuid(),
                    ExamManementId = management.Id,
                    ExamPaperDetailId = detail.Id,
                    IsMultiChoice = isMCQ,
                    Point = (i % 2 == 0) ? question.Point : 0m, // Sửa thành 0m thay vì 0
                    Feedback = (i % 2 == 0) ? "Rất tốt!" : "Cần cố gắng hơn."
                };

                if (isMCQ)
                {
                    var ans = mcAnswers.First(a => a.QuestionId == question.Id && a.IsCorrect == (i % 2 == 0));
                    submissionDetail.MultipleChoiceAnswerId = ans.Id;

                    // THÊM DÒNG NÀY ĐỂ FIX LỖI:
                    submissionDetail.Answer = "";
                }
                else
                {
                    submissionDetail.Answer =
                        (i % 2 == 0) ? "Đây là bài luận rất xuất sắc..." : "Bài làm sơ sài, thiếu ý chính.";
                }

                managementDetails.Add(submissionDetail);
            }
        }

        await context.ExamManagements.AddRangeAsync(examManagements);
        await context.ExamManagementDetails.AddRangeAsync(managementDetails);


        // 6. BÌNH LUẬN VỀ ĐỀ THI (EXAM_COMMENT) - TẠO 10 BÌNH LUẬN
        var comments = new List<ExamComment>();
        var sampleComments = new[]
        {
            "Đề thi hay quá!", "Đề này câu 2 hơi khó hiểu.", "Thời gian làm bài quá ngắn.",
            "Barem chấm điểm khá chuẩn.", "Giảng viên ra đề rất sát chương trình."
        };

        // Tạo 5 bình luận gốc
        for (int i = 0; i < 5; i++)
        {
            var parentComment = new ExamComment
            {
                Id = Guid.NewGuid(),
                ExamPaperId = examPapers[i].Id,
                ParentExamCommentId = null, // Bình luận gốc thì Parent ID là null
                Content = sampleComments[i] + " (Bình luận gốc)",
                CreatedAt = now.AddMinutes(-50 + i),
                NumberOfLikes = i * 2
            };
            comments.Add(parentComment);

            // Tạo 1 bình luận phản hồi (reply) cho chính bình luận gốc ở trên
            var replyComment = new ExamComment
            {
                Id = Guid.NewGuid(),
                ExamPaperId = examPapers[i].Id,
                ParentExamCommentId = parentComment.Id, // Gán ID của bình luận gốc làm Parent
                Content = $"Mình cũng thấy vậy! (Phản hồi bình luận {i + 1})",
                CreatedAt = now.AddMinutes(-20 + i),
                NumberOfLikes = 1
            };
            comments.Add(replyComment);
        }

        await context.ExamComments.AddRangeAsync(comments);

        // LƯU TOÀN BỘ VÀO DATABASE
        await context.SaveChangesAsync();
    }


    // HELPERS


    private static User MakeUser(string firstName, string lastName,
        string email, string phone, UserRole role, string password, DateTimeOffset now)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Role = role,
            Status = UserStatus.Active,
            Verified = true,
            VerifiedCode = 0,
            PasswordHash = HashPassword(password),
            CreatedAt = now,
        };
    }

    private static Course MakeCourse(Guid lecId, string name, string description,
        decimal price, CourseType type, int maxStudents, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        LecId = lecId,
        CourseName = name,
        Description = description,
        BasePrice = price,
        ImgUrl = $"https://placehold.co/600x400?text={Uri.EscapeDataString(name[..Math.Min(name.Length, 10)])}",
        CourseType = type,
        IsActive = true,
        StartAt = now,
        EndAt = now.AddMonths(4),
        MaxStudents = maxStudents,
        AcademicYear = 2025,
        CreatedAt = now,
    };
}