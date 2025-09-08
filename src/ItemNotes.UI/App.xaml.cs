using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ItemNotes.Application.Interfaces;
using ItemNotes.Application.Services;
using ItemNotes.Infrastructure.Data;
using ItemNotes.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace ItemNotes.UI
{
    /// <summary>
    /// Uygulamanın giriş noktası ve DI yapılandırması.
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Host builder oluşturarak DI ve konfigürasyon yapılandırılır
                var builder = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.SetBasePath(AppContext.BaseDirectory);
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // EF Core: MSSQL bağlantısı
                        services.AddDbContext<ItemNotesDbContext>(options =>
                            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

                        // Repository ve servisler
                        services.AddScoped<INoteRepository, NoteRepository>();
                        services.AddScoped<INoteService, NoteService>();

                        // ViewModel ve View kayıtları
                        services.AddTransient<ViewModels.MainWindowViewModel>();
                        services.AddTransient<ViewModels.CreateNoteWindowViewModel>();
                        services.AddTransient<ViewModels.NoteWindowViewModel>();

                        services.AddTransient<Views.MainWindow>();
                        services.AddTransient<Views.CreateNoteWindow>();
                        services.AddTransient<Views.NoteWindow>();
                    });

                _host = builder.Build();

                // Ana pencereyi DI üzerinden çöz
                var mainWindow = _host.Services.GetRequiredService<Views.MainWindow>();
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}