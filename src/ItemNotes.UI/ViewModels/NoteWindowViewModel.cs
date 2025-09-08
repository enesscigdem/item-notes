using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using ItemNotes.Application.Interfaces;
using ItemNotes.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ReactiveUI;

namespace ItemNotes.UI.ViewModels
{
    /// <summary>
    /// Not düzenleme penceresinin ViewModel'i. Sayfalar ve biçimlendirme komutları burada tanımlanır.
    /// </summary>
    public class NoteWindowViewModel : ReactiveObject
    {
        private readonly INoteService _noteService;

        public NoteWindowViewModel(INoteService noteService)
        {
            _noteService = noteService;
            Pages = new ObservableCollection<PageViewModel>();

            BoldCommand = ReactiveCommand.Create(OnBold);
            ItalicCommand = ReactiveCommand.Create(OnItalic);
            UnderlineCommand = ReactiveCommand.Create(OnUnderline);
            StrikethroughCommand = ReactiveCommand.Create(OnStrikethrough);
            BulletListCommand = ReactiveCommand.Create(OnBulletList);
            InsertImageCommand = ReactiveCommand.CreateFromTask(OnInsertImageAsync);
            InsertTableCommand = ReactiveCommand.Create(OnInsertTable);
            InsertSymbolCommand = ReactiveCommand.Create(OnInsertSymbol);
            InsertLinkCommand = ReactiveCommand.Create(OnInsertLink);
            AddPageCommand = ReactiveCommand.CreateFromTask(OnAddPageAsync);
        }

        private Note? _note;
        public Note? Note
        {
            get => _note;
            private set => this.RaiseAndSetIfChanged(ref _note, value);
        }

        public ObservableCollection<PageViewModel> Pages { get; }

        private PageViewModel? _selectedPage;
        public PageViewModel? SelectedPage
        {
            get => _selectedPage;
            set => this.RaiseAndSetIfChanged(ref _selectedPage, value);
        }

        // Biçimlendirme komutları
        public ReactiveCommand<Unit, Unit> BoldCommand { get; }
        public ReactiveCommand<Unit, Unit> ItalicCommand { get; }
        public ReactiveCommand<Unit, Unit> UnderlineCommand { get; }
        public ReactiveCommand<Unit, Unit> StrikethroughCommand { get; }
        public ReactiveCommand<Unit, Unit> BulletListCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertImageCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertTableCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertSymbolCommand { get; }
        public ReactiveCommand<Unit, Unit> InsertLinkCommand { get; }
        public ReactiveCommand<Unit, Unit> AddPageCommand { get; }

        /// <summary>
        /// Servisi dışarıya açmak için property. NoteWindow kod-behind'inde kaydetme yapılırken gerekir.
        /// </summary>
        public INoteService NoteService => _noteService;

        /// <summary>
        /// Verilen nota ait verileri yükler ve sayfaları oluşturur.
        /// </summary>
        public async Task LoadNoteAsync(Guid noteId)
        {
            var note = await _noteService.GetNoteAsync(noteId);
            if (note == null) return;
            Note = note;
            Pages.Clear();
            foreach (var page in note.Pages.OrderBy(p => p.PageIndex))
            {
                var doc = new FlowDocument();
                if (!string.IsNullOrWhiteSpace(page.Content))
                {
                    // Varsayılan olarak içeriği basit paragraf içinde gösteriyoruz.
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run(page.Content));
                    doc.Blocks.Add(paragraph);
                }
                else
                {
                    doc.Blocks.Add(new Paragraph());
                }
                Pages.Add(new PageViewModel(page.Id, page.PageIndex, doc, page.IsReadOnly));
            }
            SelectedPage = Pages.FirstOrDefault();
        }

        private void OnBold() => SelectedPage?.ToggleBold();
        private void OnItalic() => SelectedPage?.ToggleItalic();
        private void OnUnderline() => SelectedPage?.ToggleUnderline();
        private void OnStrikethrough() => SelectedPage?.ToggleStrikethrough();
        private void OnBulletList() => SelectedPage?.InsertBullet();

        private async Task OnInsertImageAsync()
        {
            if (SelectedPage != null)
            {
                await SelectedPage.InsertImageAsync();
            }
        }

        private void OnInsertTable() => SelectedPage?.InsertTable(3, 3);
        private void OnInsertSymbol() => SelectedPage?.InsertSymbol('©');
        private void OnInsertLink() => SelectedPage?.InsertLink("https://");

        private async Task OnAddPageAsync()
        {
            if (Note != null)
            {
                await _noteService.AddPageToNoteAsync(Note.Id);
                await LoadNoteAsync(Note.Id);
            }
        }
    }

    /// <summary>
    /// Bir sayfayı temsil eden yardımcı viewmodel. Biçimlendirme işlemleri burada yapılır.
    /// </summary>
    public class PageViewModel : ReactiveObject
    {
        public Guid PageId { get; }
        public int PageIndex { get; }

        private FlowDocument _document;
        public FlowDocument Document
        {
            get => _document;
            set => this.RaiseAndSetIfChanged(ref _document, value);
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
        }

        public string Header => $"Sayfa {PageIndex}";

        public PageViewModel(Guid id, int index, FlowDocument document, bool isReadOnly)
        {
            PageId = id;
            PageIndex = index;
            _document = document;
            _isReadOnly = isReadOnly;
        }

        public void ToggleBold()
        {
            if (IsReadOnly) return;
            var selection = Document.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentWeight = selection.GetPropertyValue(TextElement.FontWeightProperty);
                var newWeight = (currentWeight is FontWeight weight && weight == FontWeights.Bold)
                    ? FontWeights.Normal
                    : FontWeights.Bold;
                selection.ApplyPropertyValue(TextElement.FontWeightProperty, newWeight);
            }
        }

        public void ToggleItalic()
        {
            if (IsReadOnly) return;
            var selection = Document.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentStyle = selection.GetPropertyValue(TextElement.FontStyleProperty);
                var newStyle = (currentStyle is FontStyle style && style == FontStyle.Italic)
                    ? FontStyle.Normal
                    : FontStyle.Italic;
                selection.ApplyPropertyValue(TextElement.FontStyleProperty, newStyle);
            }
        }

        public void ToggleUnderline()
        {
            if (IsReadOnly) return;
            var selection = Document.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentDecorations = selection.GetPropertyValue(Inline.TextDecorationsProperty);
                bool hasUnderline = currentDecorations is TextDecorationCollection decs && decs.Contains(TextDecorations.Underline[0]);
                var newDecorations = new TextDecorationCollection(currentDecorations as TextDecorationCollection ?? new TextDecorationCollection());
                if (hasUnderline)
                {
                    newDecorations.Remove(TextDecorations.Underline[0]);
                }
                else
                {
                    newDecorations.Add(TextDecorations.Underline[0]);
                }
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, newDecorations);
            }
        }

        public void ToggleStrikethrough()
        {
            if (IsReadOnly) return;
            var selection = Document.Selection;
            if (selection != null && !selection.IsEmpty)
            {
                var currentDecorations = selection.GetPropertyValue(Inline.TextDecorationsProperty);
                bool hasStrike = currentDecorations is TextDecorationCollection decs && decs.Contains(TextDecorations.Strikethrough[0]);
                var newDecorations = new TextDecorationCollection(currentDecorations as TextDecorationCollection ?? new TextDecorationCollection());
                if (hasStrike)
                {
                    newDecorations.Remove(TextDecorations.Strikethrough[0]);
                }
                else
                {
                    newDecorations.Add(TextDecorations.Strikethrough[0]);
                }
                selection.ApplyPropertyValue(Inline.TextDecorationsProperty, newDecorations);
            }
        }

        public void InsertBullet()
        {
            if (IsReadOnly) return;
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run("• "));
            Document.Blocks.Add(paragraph);
        }

        public async Task InsertImageAsync()
        {
            if (IsReadOnly) return;
            var dialog = new OpenFileDialog
            {
                Title = "Resim Seç",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Resimler", Extensions = { "png", "jpg", "jpeg", "gif", "bmp" } }
                },
                AllowMultiple = false
            };
            var window = GetHostingWindow();
            if (window != null)
            {
                var result = await dialog.ShowAsync(window);
                if (result != null && result.Length > 0)
                {
                    var path = result[0];
                    var bitmap = new Avalonia.Media.Imaging.Bitmap(path);
                    var image = new Image { Source = bitmap, Width = 200 };
                    var container = new InlineUIContainer { Child = image };
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(container);
                    Document.Blocks.Add(paragraph);
                }
            }
        }

        public void InsertTable(int rows, int columns)
        {
            if (IsReadOnly) return;
            var table = new Table();
            for (int i = 0; i < columns; i++)
            {
                table.Columns.Add(new TableColumn());
            }
            var rowGroup = new TableRowGroup();
            for (int r = 0; r < rows; r++)
            {
                var row = new TableRow();
                for (int c = 0; c < columns; c++)
                {
                    var cell = new TableCell();
                    cell.Blocks.Add(new Paragraph(new Run("")));
                    row.Cells.Add(cell);
                }
                rowGroup.Rows.Add(row);
            }
            table.RowGroups.Add(rowGroup);
            Document.Blocks.Add(table);
        }

        public void InsertSymbol(char symbol)
        {
            if (IsReadOnly) return;
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(symbol.ToString()));
            Document.Blocks.Add(paragraph);
        }

        public void InsertLink(string url)
        {
            if (IsReadOnly) return;
            var hyperlink = new Hyperlink { NavigateUri = new Uri(url) };
            hyperlink.Inlines.Add(new Run(url));
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(hyperlink);
            Document.Blocks.Add(paragraph);
        }

        private Window? GetHostingWindow()
        {
            return (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        }
    }
}