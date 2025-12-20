using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GymProje.Data;   
using GymProje.Models;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Baðlantý cümlesi (DefaultConnection) bulunamadý.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<Kullanici, IdentityRole>(options =>
{
   
    options.Password.RequireDigit = false;          
    options.Password.RequireLowercase = false;       
    options.Password.RequireUppercase = false;       
    options.Password.RequireNonAlphanumeric = false; 
    options.Password.RequiredLength = 3;             

    options.User.RequireUniqueEmail = true;          // Ayný e-posta ile tekrar kayýt olunamasýn
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddControllersWithViews();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    await VeriBaslatici.Baslat(services);
}

    app.Run();