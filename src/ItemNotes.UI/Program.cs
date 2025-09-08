using System;
using Avalonia;
using Avalonia.ReactiveUI;

namespace ItemNotes.UI
{
    /// <summary>
    /// Uygulamanın giriş noktası. Avalonia konfigürasyonunu yapar ve masaüstü yaşam döngüsünü başlatır.
    /// </summary>
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();
    }
}