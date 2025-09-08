using Avalonia.Controls;
using ItemNotes.UI.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvRichTextBox;
using ItemNotes.Domain.Enums;
using ItemNotes.UI.Converters;

namespace ItemNotes.UI.Views
{
    /// <summary>
    /// Not düzenleme penceresi. Not yükleme ve kapatma sırasında verilerin kaydedilmesini sağlar.
    /// </summary>
    public partial class NoteWindow : Window
    {
        private readonly NoteWindowViewModel _viewModel;

        public NoteWindow(NoteWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            DataContext = _viewModel;
        }

        /// <summary>
        /// Belirtilen notu yükler ve diyaloğu gösterir.
        /// </summary>
        /// <param name="noteId">Notun benzersiz kimliği.</param>
        /// <param name="owner">Sahip pencere.</param>
        public async Task ShowNoteAsync(Guid noteId, Window owner)
        {
            await _viewModel.LoadNoteAsync(noteId);
            await ShowDialog(owner);
        }

        protected override async void OnClosing(WindowClosingEventArgs e)
        {
            // pencere kapanırken mevcut sayfa içeriklerini güncelle
            if (_viewModel.Note != null)
            {
                foreach (var page in _viewModel.Note.Pages)
                {
                    var vm = _viewModel.Pages.FirstOrDefault(p => p.PageId == page.Id);
                    if (vm != null)
                    {
                        // Dokümanı basit metin olarak sakla
                        page.Content = ExtractPlainText(vm.Document);
                    }
                }
                await _viewModel.NoteService.UpdateNoteAsync(_viewModel.Note);
            }
            base.OnClosing(e);
        }

        private string ExtractPlainText(FlowDocument document)
        {
            // Basit olarak FlowDocument içindeki tüm blokların metinlerini birleştiriyoruz.
            var texts = document.Blocks.OfType<Block>().Select(b =>
            {
                if (b is Paragraph p)
                {
                    return string.Join(string.Empty, p.Inlines.OfType<Inline>().Select(inl => inl is Run r ? r.Text : string.Empty));
                }
                return string.Empty;
            });
            return string.Join("\n", texts);
        }
        public void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        // Renk değiştirme menüsü – notun rengini günceller ve veritabanına kaydeder
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

        private async Task ChangeColor(NoteColor color)
        {
            if (_viewModel.Note != null && _viewModel.Note.Color != color)
            {
                _viewModel.Note.Color = color;
                await _viewModel.NoteService.UpdateNoteAsync(_viewModel.Note);
                // Arayüz rengini güncellemek için DataContext’e bildirim gönderebilirsiniz
                this.Background = new NoteColorToBrushConverter().Convert(color, typeof(IBrush), null, CultureInfo.InvariantCulture) as IBrush;
            }
        }
    }
}