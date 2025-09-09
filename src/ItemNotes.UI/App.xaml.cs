using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

// Application & Infrastructure
using ItemNotes.Application.Interfaces;
using ItemNotes.Application.Services;
using ItemNotes.Infrastructure.Repositories;
using ItemNotes.Infrastructure.Data;

// UI
using ItemNotes.UI.ViewModels;
using ItemNotes.UI.Views;

namespace ItemNotes.UI;

public partial class App : Avalonia.Application
{
    private IServiceProvider? _sp;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _sp = BuildServiceProvider();

            // ---- DB hazırla (migration/oluştur) ----
            using (var scope = _sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ItemNotesDbContext>();
                try { db.Database.Migrate(); } catch { /* burada hata verirseniz connection stringi kontrol edin */ }
            }

            // ---- MainWindow’u DI’dan al ----
            var main = _sp.GetRequiredService<MainWindow>();
            desktop.MainWindow = main;
        }
        base.OnFrameworkInitializationCompleted();
    }

    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // ---- Configuration ----
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        services.AddSingleton<IConfiguration>(config);

        var connStr = config.GetConnectionString("DefaultConnection")
                     ?? Environment.GetEnvironmentVariable("ITEMNOTES_CONNECTION")
                     ?? "Server=localhost;Database=item_notes_db;User Id=sa;Password=reallyStrongPwd123;TrustServerCertificate=true";

        // ---- EF Core ----
        services.AddDbContext<ItemNotesDbContext>(opt => opt.UseSqlServer(connStr));

        // ---- Repo & Service ----
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<INoteService, NoteService>();

        // ---- ViewModels ----
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<CreateNoteWindowViewModel>();
        services.AddTransient<NoteWindowViewModel>();

        // ---- Views ----
        services.AddTransient<MainWindow>();
        services.AddTransient<CreateNoteWindow>();
        services.AddTransient<NoteWindow>();

        return services.BuildServiceProvider();
    }
}
