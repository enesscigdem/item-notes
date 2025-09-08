using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ItemNotes.Application.Interfaces;
using ItemNotes.Domain.Entities;
using ReactiveUI;
using AvRichTextBox;

// ---- Alias'lar: AvRichTextBox tarafı ----
using AvFlowDocument = AvRichTextBox.FlowDocument;
using AvParagraph = AvRichTextBox.Paragraph;
using AvEditableRun = AvRichTextBox.EditableRun;
using AvEditableInlineUIContainer = AvRichTextBox.EditableInlineUIContainer;
using AvTextRange = AvRichTextBox.TextRange;

namespace ItemNotes.UI.ViewModels
{
    public class NoteWindowViewModel : ReactiveObject
    {
        private readonly INoteService _noteService;

        public NoteWindowViewModel(INoteService noteService)
        {
            _noteService = noteService;
            Pages = new ObservableCollection<PageViewModel>();

            BoldCommand          = ReactiveCommand.Create(() => SelectedPage?.ApplyBold());
            ItalicCommand        = ReactiveCommand.Create(() => SelectedPage?.ApplyItalic());
            UnderlineCommand     = ReactiveCommand.Create(() => SelectedPage?.ApplyUnderline());
            StrikethroughCommand = ReactiveCommand.Create(() => SelectedPage?.ApplyStrikethrough());
            BulletListCommand    = ReactiveCommand.Create(() => SelectedPage?.InsertBullet());
            InsertImageCommand   = ReactiveCommand.CreateFromTask(() => SelectedPage?.InsertImageAsync() ?? Task.CompletedTask);
            InsertTableCommand   = ReactiveCommand.Create(() => SelectedPage?.InsertPseudoTable(3, 3));
            InsertSymbolCommand  = ReactiveCommand.Create(() => SelectedPage?.InsertSymbol('©'));
            InsertLinkCommand    = ReactiveCommand.Create(() => SelectedPage?.InsertLink("https://"));
            AddPageCommand       = ReactiveCommand.CreateFromTask(OnAddPageAsync);
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

        public INoteService NoteService => _noteService;

        public async Task LoadNoteAsync(Guid noteId)
        {
            var note = await _noteService.GetNoteAsync(noteId);
            if (note == null) return;

            Note = note;
            Pages.Clear();

            foreach (var page in note.Pages.OrderBy(p => p.PageIndex))
            {
                var doc = new AvFlowDocument();
                var p   = new AvParagraph();
                p.Inlines.Add(new AvEditableRun(string.IsNullOrWhiteSpace(page.Content) ? "" : page.Content));
                doc.Blocks.Add(p);

                Pages.Add(new PageViewModel(page.Id, page.PageIndex, doc, page.IsReadOnly));
            }

            SelectedPage = Pages.FirstOrDefault();
        }

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

        private async Task OnAddPageAsync()
        {
            if (Note is null) return;
            await _noteService.AddPageToNoteAsync(Note.Id);
            await LoadNoteAsync(Note.Id);
        }
    }

    public class PageViewModel : ReactiveObject
    {
        public Guid PageId { get; }
        public int PageIndex { get; }
        public string Header => $"Sayfa {PageIndex}";

        private AvFlowDocument _document;
        public AvFlowDocument Document
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

        public PageViewModel(Guid id, int index, AvFlowDocument document, bool isReadOnly)
        {
            PageId = id;
            PageIndex = index;
            _document = document;
            _isReadOnly = isReadOnly;
        }

        private AvTextRange? Sel => Document?.Selection; // AvRichTextBox Selection

        public void ApplyBold()
        {
            if (IsReadOnly || Sel is null || Sel.Length == 0) return;
            Sel.ApplyFormatting(TextElement.FontWeightProperty, FontWeight.Bold);
        }

        public void ApplyItalic()
        {
            if (IsReadOnly || Sel is null || Sel.Length == 0) return;
            Sel.ApplyFormatting(TextElement.FontStyleProperty, FontStyle.Italic);
        }

        public void ApplyUnderline()
        {
            if (IsReadOnly || Sel is null || Sel.Length == 0) return;
            Sel.ApplyFormatting(Inline.TextDecorationsProperty, TextDecorations.Underline);
        }

        public void ApplyStrikethrough()
        {
            if (IsReadOnly || Sel is null || Sel.Length == 0) return;
            Sel.ApplyFormatting(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
        }

        public void InsertBullet()
        {
            if (IsReadOnly) return;
            var p = new AvParagraph();
            p.Inlines.Add(new AvEditableRun("• "));
            Document.Blocks.Add(p);
        }

        public async Task InsertImageAsync()
        {
            if (IsReadOnly) return;

            var dialog = new OpenFileDialog
            {
                Title = "Resim Seç",
                Filters = new List<FileDialogFilter>
                {
                    new() { Name = "Resimler", Extensions = { "png", "jpg", "jpeg", "gif", "bmp" } }
                },
                AllowMultiple = false
            };

            var window =
                (global::Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
                ?.MainWindow;

            var files = window is null ? null : await dialog.ShowAsync(window);
            if (files is null || files.Length == 0) return;

            var img = new Image { Source = new Bitmap(files[0]), Width = 200 };

            var editable = new AvEditableInlineUIContainer(img);

            var p = new AvParagraph();
            p.Inlines.Add(editable);
            Document.Blocks.Add(p);
        }

        // RichTextBox'ta henüz gerçek Table yok; geçici "pseudo" metin
        public void InsertPseudoTable(int rows, int columns)
        {
            if (IsReadOnly) return;
            var p = new AvParagraph();
            p.Inlines.Add(new AvEditableRun($"[Tablo {rows}x{columns}]  |  hücreleri metinle doldurun"));
            Document.Blocks.Add(p);
        }

        public void InsertSymbol(char symbol)
        {
            if (IsReadOnly) return;
            var p = new AvParagraph();
            p.Inlines.Add(new AvEditableRun(symbol.ToString()));
            Document.Blocks.Add(p);
        }

        public void InsertLink(string url)
        {
            if (IsReadOnly) return;

            var link = new HyperlinkButton { Content = url };
            link.Click += (_, __) =>
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true };
                    System.Diagnostics.Process.Start(psi);
                }
                catch { /* yut */ }
            };

            var editable = new AvEditableInlineUIContainer(link);

            var p = new AvParagraph();
            p.Inlines.Add(editable);
            Document.Blocks.Add(p);
        }
    }
}