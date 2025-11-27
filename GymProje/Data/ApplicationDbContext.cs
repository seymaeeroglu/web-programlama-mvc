using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GymProje.Models; // Kendi proje adın GymProje ise

namespace GymProje.Data
{
    public class ApplicationDbContext : IdentityDbContext<Kullanici>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Uzmanlik> Uzmanliklar { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Ücret (Decimal) Hatası Çözümü
            builder.Entity<Hizmet>()
                .Property(h => h.Ucret)
                .HasColumnType("decimal(18,2)");

            // 2. ÇAKIŞMA (CYCLE) HATASI ÇÖZÜMÜ:
            // "Bir Hizmet'in bir Uzmanlığı olur. Eğer Uzmanlık silinirse,
            // Hizmetleri otomatik silme (Restrict), hata fırlat."
            builder.Entity<Hizmet>()
                .HasOne(h => h.Uzmanlik)
                .WithMany()
                .HasForeignKey(h => h.UzmanlikId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}