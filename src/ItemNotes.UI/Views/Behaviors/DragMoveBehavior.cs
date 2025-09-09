// Views/Behaviors/DragMoveBehavior.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace ItemNotes.UI.Views.Behaviors;

/// <summary>
/// İçerikteki herhangi bir alanı sürükleyerek UserControl'ü taşıyabilme.
/// Pencere içinde çalışır; üst başlığa ekleyin (NoteDetailView başlığına uygulandı).
/// </summary>
public static class DragMoveBehavior
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsEnabled", typeof(DragMoveBehavior));

    public static void SetIsEnabled(AvaloniaObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static bool GetIsEnabled(AvaloniaObject element) => element.GetValue(IsEnabledProperty);

    static DragMoveBehavior()
    {
        IsEnabledProperty.Changed.Subscribe(args =>
        {
            if (args.Sender is Control c)
            {
                if (args.NewValue.GetValueOrDefault<bool>())
                {
                    c.PointerPressed += OnPressed;
                }
                else
                {
                    c.PointerPressed -= OnPressed;
                }
            }
        });
    }

    private static void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control ctrl) return;
        if (!GetIsEnabled(ctrl)) return;

        if (ctrl.GetVisualRoot() is Window win)
        {
            // Sürükleme başlasın
            win.BeginMoveDrag(e);
        }
    }
}