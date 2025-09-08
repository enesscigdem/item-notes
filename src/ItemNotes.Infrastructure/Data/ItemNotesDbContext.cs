using ItemNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ItemNotes.Infrastructure.Data
{
    /// <summary>
    /// Uygulamanın Entity Framework Core veri bağlamı. SQL Server kullanır.
    /// </summary>
    public class ItemNotesDbContext : DbContext
    {
        public ItemNotesDbContext(DbContextOptions<ItemNotesDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Tablolar: Notlar.
        /// </summary>
        public DbSet<Note> Notes => Set<Note>();

        /// <summary>
        /// Tablolar: Not sayfaları.
        /// </summary>
        public DbSet<NotePage> NotePages => Set<NotePage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Note konfigürasyonu
            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Color).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("getutcdate()");

                entity.HasMany(e => e.Pages)
                      .WithOne(p => p.Note)
                      .HasForeignKey(p => p.NoteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // NotePage konfigürasyonu
            modelBuilder.Entity<NotePage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).HasColumnType("nvarchar(max)");
                entity.Property(e => e.PageIndex).IsRequired();
                entity.Property(e => e.IsReadOnly).HasDefaultValue(false);
            });
        }
    }
}