using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.WebView.Desktop; // <- önemli

namespace ItemNotes.UI;

internal static class Program
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI()
            .UseDesktopWebView(); // <- önemli

    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
}