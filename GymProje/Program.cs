using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymProje.Data;   // DÝKKAT: Burayý kendi proje adýnýzla deðiþtirin (Örn: SporSalonuApp.Data)
using GymProje.Models; // DÝKKAT: Burayý kendi proje adýnýzla deðiþtirin

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Baðlantý cümlesi (DefaultConnection) bulunamadý.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<Kullanici, IdentityRole>(options =>
{
    // Geliþtirme aþamasýnda þifre kurallarýný basitleþtiriyoruz (Admin þifresi 'sau' olabilsin diye)
    options.Password.RequireDigit = false;           // Rakam zorunluluðu yok
    options.Password.RequireLowercase = false;       // Küçük harf zorunluluðu yok
    options.Password.RequireUppercase = false;       // Büyük harf zorunluluðu yok
    options.Password.RequireNonAlphanumeric = false; // Sembol (!, @ vb.) zorunluluðu yok
    options.Password.RequiredLength = 3;             // En az 3 karakter olsun

    options.User.RequireUniqueEmail = true;          // Ayný e-posta ile tekrar kayýt olunamasýn
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// MVC Servisleri
builder.Services.AddControllersWithViews();

var app = builder.Build();

// HTTP Ýstek Hattý (Pipeline) Ayarlarý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// DÝKKAT: Kimlik Doðrulama (Authentication) Yetkilendirme'den (Authorization) ÖNCE gelmelidir.
app.UseAuthentication();
app.UseAuthorization();

// Varsayýlan Rota
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// --- BAÞLANGIÇ VERÝLERÝNÝ YÜKLEME KODU ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // VeriBaslatici sýnýfýndaki Baslat metodunu çaðýrýyoruz
    // await kullanabilmek için main metodunun async olmasý gerekebilir ama
    // .NET 6+ top-level statements bunu otomatik halleder, sadece Wait() diyelim garanti olsun.
    await VeriBaslatici.Baslat(services);
}

    app.Run();