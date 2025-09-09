// Views/Behaviors/DebounceSaveBehavior.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ItemNotes.UI.Views.Behaviors;

/// <summary>
/// TextBox.Text değiştikçe doğrudan kaydetmek yerine, kullanıcı yazmayı bırakınca (debounce) kaydeder.
/// </summary>
public static class DebounceSaveBehavior
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>("IsEnabled", typeof(DebounceSaveBehavior));

    public static readonly AttachedProperty<int> IntervalMillisecondsProperty =
        AvaloniaProperty.RegisterAttached<TextBox, int>("IntervalMilliseconds", typeof(DebounceSaveBehavior), 600);

    private static readonly AttachedProperty<CancellationTokenSource?> CtsProperty =
        AvaloniaProperty.RegisterAttached<TextBox, CancellationTokenSource?>("Cts", typeof(DebounceSaveBehavior));

    public static void SetIsEnabled(AvaloniaObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static bool GetIsEnabled(AvaloniaObject element) => element.GetValue(IsEnabledProperty);

    public static void SetIntervalMilliseconds(AvaloniaObject element, int value) => element.SetValue(IntervalMillisecondsProperty, value);
    public static int GetIntervalMilliseconds(AvaloniaObject element) => element.GetValue(IntervalMillisecondsProperty);

    static DebounceSaveBehavior()
    {
        IsEnabledProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is TextBox tb)
            {
                if (args.NewValue.GetValueOrDefault<bool>())
                {
                    tb.AddHandler(TextBox.TextChangedEvent, OnTextChanged, RoutingStrategies.Bubble);
                    tb.AttachedToVisualTree += OnAttach;
                    tb.DetachedFromVisualTree += OnDetach;
                }
                else
                {
                    tb.RemoveHandler(TextBox.TextChangedEvent, OnTextChanged);
                    tb.AttachedToVisualTree -= OnAttach;
                    tb.DetachedFromVisualTree -= OnDetach;
                }
            }
        });
    }

    private static void OnAttach(object? sender, VisualTreeAttachmentEventArgs e) { }
    private static void OnDetach(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is TextBox tb && tb.GetValue(CtsProperty) is { } cts)
        {
            cts.Cancel();
            tb.SetValue(CtsProperty, null);
        }
    }

    private static void OnTextChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (!GetIsEnabled(tb)) return;

        var interval = GetIntervalMilliseconds(tb);
        tb.GetValue(CtsProperty)?.Cancel();
        var cts = new CancellationTokenSource();
        tb.SetValue(CtsProperty, cts);
        _ = DebouncedSaveAsync(tb, interval, cts.Token);
    }

    private static async Task DebouncedSaveAsync(TextBox tb, int delayMs, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delayMs, ct);
            if (ct.IsCancellationRequested) return;

            if (tb.DataContext is ItemNotes.UI.ViewModels.NoteWindowViewModel vm)
            {
                await vm.SaveAllPagesAsync(); // VM içinde servis çağrısı
            }
        }
        catch (TaskCanceledException) { /* yoksay */ }
    }
}
