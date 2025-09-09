// Views/Behaviors/EnterAfterImageFixBehavior.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace ItemNotes.UI.Views.Behaviors;

/// <summary>
/// Bazı durumlarda metin içinde resim (inline tag gibi) sonrası Enter basınca kontrol çöker.
/// Bu davranış Enter'ı yakalayıp güvenli şekilde yeni satır ekler.
/// Not: İçerik TextBox olduğu için "resim" temsili markdown gibi düşünülür: ![alt](path)
/// </summary>
public static class EnterAfterImageFixBehavior
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>("IsEnabled", typeof(EnterAfterImageFixBehavior));

    public static void SetIsEnabled(AvaloniaObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static bool GetIsEnabled(AvaloniaObject element) => element.GetValue(IsEnabledProperty);

    static EnterAfterImageFixBehavior()
    {
        IsEnabledProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is TextBox tb)
            {
                if (args.NewValue.GetValueOrDefault<bool>())
                    tb.AddHandler(InputElement.KeyDownEvent, OnKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                else
                    tb.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
            }
        });
    }

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (!GetIsEnabled(tb)) return;

        if (e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None)
        {
            // Kuvvetli koruma: İmleç pozisyonunda markdown-IMAJ paterni varsa, önce normal bir satır sonu ekle
            // ve caret'i güvenli pozisyona taşı.
            var text = tb.Text ?? string.Empty;
            var idx = tb.CaretIndex;
            if (idx > 0 && idx <= text.Length)
            {
                // Basit bir heuristik: caret öncesi/sonrası görüntü link paterni(![...](...))
                var left = idx > 0 ? text[idx - 1] : '\0';
                var right = idx < text.Length ? text[idx] : '\0';

                var aroundImage = left == ')' || right == ')'; // ![alt](path)
                if (aroundImage)
                {
                    tb.Text = text.Insert(idx, "\n");
                    tb.CaretIndex = idx + 1;
                    e.Handled = true;
                }
            }
        }
    }
}
