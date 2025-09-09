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
    private IServiceProvider? _serviceProvider;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _serviceProvider = BuildServiceProvider();

            var vm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            desktop.MainWindow.DataContext = vm;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // --- Configuration ---
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        services.AddSingleton<IConfiguration>(config);

        var connStr =
            config.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ITEMNOTES_CONNECTION")
            ?? "Server=localhost;Database=item_notes_db;User Id=sa;Password=reallyStrongPwd123;TrustServerCertificate=true";

        // --- EF Core ---
        services.AddDbContext<ItemNotesDbContext>(opt => opt.UseSqlServer(connStr));

        // --- Repos & Services ---
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<INoteService, NoteService>();

        // --- ViewModels ---
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<CreateNoteWindowViewModel>();
        services.AddTransient<NoteWindowViewModel>();

        // --- Views (Windows) ---
        services.AddTransient<MainWindow>();
        services.AddTransient<CreateNoteWindow>();
        services.AddTransient<NoteWindow>();      // <—— burada < > zorunlu, hatayı yapan buydu

        return services.BuildServiceProvider();
    }
}
