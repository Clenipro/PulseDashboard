using Microsoft.EntityFrameworkCore;
using ApiChamada.Models;

namespace ApiChamada.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapear a entidade User para a tabela 'user'
            modelBuilder.Entity<User>().ToTable("user");

            // Mapear a entidade RefreshToken para a tabela 'refreshtoken'
            modelBuilder.Entity<RefreshToken>().ToTable("refreshtoken");

            // Configurações para a entidade User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Email).HasMaxLength(191).IsRequired();
                entity.Property(e => e.Nome).HasMaxLength(100);
                entity.Property(e => e.Username).HasMaxLength(191).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(191).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasConversion(
                    v => v,
                    v => v.ToUpper()
                );
                entity.Property(e => e.CreatedAt).HasColumnType("datetime(3)");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime(3)");
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Configurações para a entidade RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(191).IsRequired();
                entity.Property(e => e.Token).HasMaxLength(400).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.ExpiresAt).HasColumnType("datetime(3)");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime(3)");
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .HasConstraintName("FK_refreshtoken_user_userId");
            });
        }
    }
}