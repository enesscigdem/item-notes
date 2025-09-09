using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvRichTextBox;
using ItemNotes.Domain.Enums;
using ItemNotes.UI.Converters;
using ItemNotes.UI.ViewModels;

// Av tip alias'ları
using AvParagraph = AvRichTextBox.Paragraph;
using AvEditableRun = AvRichTextBox.EditableRun;
using AvEditableInlineUIContainer = AvRichTextBox.EditableInlineUIContainer;

namespace ItemNotes.UI.Views
{
    public partial class NoteWindow : Window
    {
        private readonly NoteWindowViewModel _viewModel;

        public NoteWindow(NoteWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            DataContext = _viewModel;

            this.Closing += async (_, __) =>
            {
                try { await _viewModel.SaveAllPagesAsync(); }
                catch { /* ignore */ }
            };
        }

        public async Task ShowNoteAsync(Guid noteId, Window owner)
        {
            await _viewModel.LoadNoteAsync(noteId);
            await ShowDialog(owner);
        }

        public void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

        public async void ChangeColorYellow(object? s, RoutedEventArgs e) => await ChangeColor(NoteColor.Yellow);
        public async void ChangeColorGreen(object? s, RoutedEventArgs e)  => await ChangeColor(NoteColor.Green);
        public async void ChangeColorRed(object? s, RoutedEventArgs e)    => await ChangeColor(NoteColor.Red);

        private async Task ChangeColor(NoteColor newColor)
        {
            if (_viewModel.Note != null && _viewModel.Note.Color != newColor)
            {
                _viewModel.Note.Color = newColor;
                await _viewModel.NoteService.UpdateNoteAsync(_viewModel.Note);

                var converter = new NoteColorToBrushConverter();
                if (this.FindControl<Border>("RootBorder") is { } border)
                {
                    border.Background = converter.Convert(newColor, typeof(IBrush), null, CultureInfo.InvariantCulture) as IBrush;
                }
            }
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            await _viewModel.SaveAllPagesAsync();
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            try { BeginMoveDrag(e); } catch { /* ignore */ }
        }

        // Enter-sonrası resim çökmesi + (Ctrl/Cmd+A çağrısı yok; AvRichTextBox'ta SelectAll yok)
        private void Editor_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not AvRichTextBox.RichTextBox editor) return;

            // Sadece Enter fix'i:
            if (e.Key == Key.Enter)
            {
                try
                {
                    var doc = editor.FlowDocument;
                    var lastBlock = doc?.Blocks.LastOrDefault();
                    if (lastBlock is AvParagraph p && p.Inlines.Count > 0 && p.Inlines.Last() is AvEditableInlineUIContainer)
                    {
                        var np = new AvParagraph();
                        np.Inlines.Add(new AvEditableRun("")); // boş satır
                        doc.Blocks.Add(np);
                        e.Handled = true;
                    }
                }
                catch { /* ignore */ }
            }
        }

        private void Editor_KeyUp(object? sender, KeyEventArgs e)
        {
            // seçim/durum güncellemesi
            try { _viewModel.SelectedPage?.RefreshSelectionStates(); } catch { }
        }

        private void Editor_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            // seçim/durum güncellemesi
            try { _viewModel.SelectedPage?.RefreshSelectionStates(); } catch { }
        }
    }
}
