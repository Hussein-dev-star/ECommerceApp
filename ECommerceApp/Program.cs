using ECommerceApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. تسجيل خدمات الـ Controllers والـ Views
builder.Services.AddControllersWithViews();

// 2. إعداد الاتصال بقاعدة البيانات (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. تسجيل خدمات الـ Identity للحسابات والصلاحيات الحقيقية
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// إعداد مسار صفحة اللوجن الافتراضية
builder.Services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/Login");

// 4. تفعيل الـ HttpContext والـ Session للسلة (تسجيل الخدمة مرة واحدة فقط)
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

// 5. إعدادات بيئة التطوير والـ Production
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// 6. ترتيب الـ Middlewares الإلزامي (مهم جداً)
app.UseAuthentication(); // التحقق من الحساب
app.UseAuthorization();  // التحقق من الصلاحيات
app.UseSession();        // تفعيل السلة المؤقتة (مكتوب مرة واحدة هنا بس)

app.MapStaticAssets();

// 7. كود توليد البيانات الافتراضية (الأدمن + الـ 10 منتجات + الأقسام)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    // أ- توليد الـ Roles والأدمن
    if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
    if (!await roleManager.RoleExistsAsync("Customer")) await roleManager.CreateAsync(new IdentityRole("Customer"));

    string adminEmail = builder.Configuration["AdminSettings:Email"] ?? "hussein@store.com";
    string adminPassword = builder.Configuration["AdminSettings:Password"] ?? "Hussein@123";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var createAdminResult = await userManager.CreateAsync(newAdmin, adminPassword);
        if (createAdminResult.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }

    // 2. تفريغ الجداول القديمة تماماً لضمان نزول الـ 10 منتجات الجداد بالصور
    if (context.Products.Any())
    {
        context.Products.RemoveRange(context.Products);
        context.SaveChanges();
    }
    if (context.Categories.Any())
    {
        context.Categories.RemoveRange(context.Categories);
        context.SaveChanges();
    }

    // 3. شحن الأقسام الجديدة من الصفر
    var electronicsCategory = new Category { Name = "إلكترونيات", Description = "أحدث الأجهزة الذكية والشاشات والملحقات" };
    var clothesCategory = new Category { Name = "ملابس", Description = "أزياء عصرية بجودة عالية لجميع المواسم" };

    context.Categories.AddRange(electronicsCategory, clothesCategory);
    context.SaveChanges(); // حفظ الأقسام عشان ناخد الـ IDs بتاعتها

    // 4. شحن الـ 10 منتجات الاحترافية بالكميات والصور أونلاين
    context.Products.AddRange(
        // --- قسم الإلكترونيات (5 منتجات) ---
        new Product
        {
            Name = "هاتف OPPO A78 الذكي",
            Price = 250.00M,
            Description = "شاشة أموليد 90 هرتز، شحن سريع 67 واط، وكاميرا 50 ميجابكسل.",
            ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500",
            CategoryId = electronicsCategory.Id
        },
        new Product
        {
            Name = "سماعة لاسلكية Sony WH-1000XM4",
            Price = 350.00M,
            Description = "سماعة رأس محيطية مزودة بأقوى نظام عزل ضوضاء وصوت عالي الدقة.",
            ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500",
            CategoryId = electronicsCategory.Id
        },
        new Product
        {
            Name = "ساعة ذكية Apple Watch Series 9",
            Price = 400.00M,
            Description = "مراقبة الأكسجين في الدم، تتبع التمارين الرياضية، وشاشة ريتنا دائماً قيد التشغيل.",
            ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500",
            CategoryId = electronicsCategory.Id
        },
        new Product
        {
            Name = "لابتوب ASUS Vivobook 15",
            Price = 700.00M,
            Description = "معالج Intel Core i5، رام 16 جيجابايت، وهارد 512 SSD فائق السرعة.",
            ImageUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=500",
            CategoryId = electronicsCategory.Id
        },
        new Product
        {
            Name = "شاحن متنقل Anker PowerBank",
            Price = 45.00M,
            Description = "سعة 20,000 مللي أمبير مع تقنية الشحن السريع وحماية الأجهزة.",
            ImageUrl = "https://images.unsplash.com/photo-1609592424109-dd9892f1b17c?w=500",
            CategoryId = electronicsCategory.Id
        },

        // --- قسم الملابس (5 منتجات) ---
        new Product
        {
            Name = "قميص كاجوال أكسفورد",
            Price = 25.00M,
            Description = "قميص قطني 100% مريح ومناسب للعمل والخروجات اليومية.",
            ImageUrl = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=500",
            CategoryId = clothesCategory.Id
        },
        new Product
        {
            Name = "جاكيت جينز عصري",
            Price = 55.00M,
            Description = "جاكيت جينز كلاسيكي متين يمنحك مظهراً شبابياً رائعاً.",
            ImageUrl = "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=500",
            CategoryId = clothesCategory.Id
        },
        new Product
        {
            Name = "حذاء رياضي مريح UltraBoost",
            Price = 120.00M,
            Description = "حذاء جري خفيف الوزن بنعل مرن يوفر راحة تامة للقدمين طوال اليوم.",
            ImageUrl = "https://images.unsplash.com/photo-1491553895911-0055eca6402d?w=500",
            CategoryId = clothesCategory.Id
        },
        new Product
        {
            Name = "بنطال تشينو كلاسيك",
            Price = 35.00M,
            Description = "بنطال بقصة سليم فيت مريحة وخامات عالية التحمل تناسب جميع الأذواق.",
            ImageUrl = "https://images.unsplash.com/photo-1624378439575-d8705ad7ae80?w=500",
            CategoryId = clothesCategory.Id
        },
        new Product
        {
            Name = "هودي شتوي مبطن",
            Price = 40.00M,
            Description = "هودي ثقيل ومبطن من الداخل يوفر دفئاً ممتازاً مع تصميم كاجوال شيك.",
            ImageUrl = "https://images.unsplash.com/photo-1556821840-3a63f95609a7?w=500",
            CategoryId = clothesCategory.Id
        }
    );
    context.SaveChanges();
}


// 8. خريطة مسارات الـ Routes للـ URLs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();