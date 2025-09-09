using Avalonia.Controls;
using Avalonia.Interactivity;
using ItemNotes.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;

namespace ItemNotes.UI.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _sp;
        private readonly MainWindowViewModel _vm;

        public MainWindow(IServiceProvider sp, MainWindowViewModel vm)
        {
            _sp = sp;
            _vm = vm;
            InitializeComponent();
            DataContext = _vm;

            // VM eventlerine abone
            _vm.CreateNoteRequested += OnCreateNoteRequested;
            _vm.OpenNoteRequested   += OnOpenNoteRequested;

            Opened += async (_, __) =>
            {
                try { await _vm.LoadNotesAsync(); }
                catch (Exception ex) { await ShowErrorAsync("Notlar yüklenemedi", ex); }
            };
        }

        // --- Click fallback'ları ---
        private void OnNewNoteClick(object? sender, RoutedEventArgs e) => OnCreateNoteRequested();

        private void OnOpenSelectedClick(object? sender, RoutedEventArgs e)
        {
            if (_vm.SelectedNote is not null)
                OnOpenNoteRequested(_vm.SelectedNote.Id);
        }

        // --- VM eventleri ---
        private async void OnCreateNoteRequested()
        {
            try
            {
                var win = _sp.GetRequiredService<CreateNoteWindow>();
                var result = await win.ShowDialog<Guid?>(this);
                if (result.HasValue)
                {
                    await _vm.LoadNotesAsync();
                    await OnOpenNoteAsync(result.Value);
                }
            }
            catch (Exception ex) { await ShowErrorAsync("Yeni Not penceresi açılamadı", ex); }
        }

        private async void OnOpenNoteRequested(Guid noteId) => await OnOpenNoteAsync(noteId);

        private async Task OnOpenNoteAsync(Guid noteId)
        {
            try
            {
                var win = _sp.GetRequiredService<NoteWindow>();
                await win.ShowNoteAsync(noteId, this);
            }
            catch (Exception ex) { await ShowErrorAsync("Not penceresi açılamadı", ex); }
        }

        // Basit hata kutusu (paketsiz)
        private async Task ShowErrorAsync(string title, Exception ex)
        {
            var dlg = new Window
            {
                Title = title,
                Width = 520,
                Height = 220,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Spacing = 12,
                    Children =
                    {
                        new TextBlock { Text = ex.Message, TextWrapping = TextWrapping.Wrap },
                        new Button { Content = "Tamam", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right }
                    }
                }
            };
            ((StackPanel)dlg.Content!).Children[1].PointerPressed += (_, _) => dlg.Close();
            await dlg.ShowDialog(this);
        }
    }
}
