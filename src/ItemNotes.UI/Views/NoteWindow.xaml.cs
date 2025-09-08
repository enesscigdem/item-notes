using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ItemNotes.Domain.Enums;
using ItemNotes.UI.Converters;
using ItemNotes.UI.ViewModels;
using System.Globalization;
using System.Threading.Tasks;

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
        }

        public async Task ShowNoteAsync(Guid noteId, Window owner)
        {
            await _viewModel.LoadNoteAsync(noteId);
            await ShowDialog(owner);
        }
        
        // “Kapat” menü öğesi
        public void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        // Renk değiştirme işleyicileri
        public async void ChangeColorYellow(object? sender, RoutedEventArgs e)
        {
            await ChangeColor(NoteColor.Yellow);
        }

        public async void ChangeColorGreen(object? sender, RoutedEventArgs e)
        {
            await ChangeColor(NoteColor.Green);
        }

        public async void ChangeColorRed(object? sender, RoutedEventArgs e)
        {
            await ChangeColor(NoteColor.Red);
        }

        private async Task ChangeColor(NoteColor newColor)
        {
            if (_viewModel.Note != null && _viewModel.Note.Color != newColor)
            {
                _viewModel.Note.Color = newColor;
                await _viewModel.NoteService.UpdateNoteAsync(_viewModel.Note);

                // Arka planı güncelle: RootBorder ismi XAML’de verilmiş olmalı
                var converter = new NoteColorToBrushConverter();
                if (this.FindControl<Border>("RootBorder") is { } border)
                {
                    border.Background = converter.Convert(newColor, typeof(IBrush), null, CultureInfo.InvariantCulture) as IBrush;
                }
            }
        }
    }
}