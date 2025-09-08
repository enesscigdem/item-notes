using System;
using System.IO;
using ItemNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ItemNotes.Infrastructure.Data
{
    public class ItemNotesDbContext : DbContext
    {
        // Uygulama çalışma zamanı (DI) için kullanılan ctor
        public ItemNotesDbContext(DbContextOptions<ItemNotesDbContext> options) : base(options)
        {
        }

        // DESIGN-TIME için parametresiz ctor (EF Tools bunu kullanır)
        public ItemNotesDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Eğer zaten SQL Server PROVIDER’ı verilmişse çık.
            // (İç tipe dokunmadan basit bir kontrol)
            var providerAlreadySet = optionsBuilder.Options.Extensions
                .Any(e => e.GetType().Name.Contains("SqlServerOptionsExtension", StringComparison.OrdinalIgnoreCase));

            if (providerAlreadySet)
                return;

            // Runtime için sağlam base path
            var basePath = AppContext.BaseDirectory;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // UI output’taki dosya
                .Build();

            var conn = configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("ITEMNOTES__CONNSTR");

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException(
                    "Connection string bulunamadı. 'appsettings.json' içine ConnectionStrings:DefaultConnection ekleyin veya ITEMNOTES__CONNSTR ortam değişkenini ayarlayın.");

            optionsBuilder.UseSqlServer(conn, b =>
                b.MigrationsAssembly(typeof(ItemNotesDbContext).Assembly.FullName));
        }


        public DbSet<Note> Notes => Set<Note>();
        public DbSet<NotePage> NotePages => Set<NotePage>();
    }
}