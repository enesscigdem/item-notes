using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ItemNotes.Application.Interfaces;
using ItemNotes.Domain.Entities;
using ReactiveUI;

namespace ItemNotes.UI.ViewModels
{
    /// <summary>
    /// Not düzenleme penceresi için ViewModel. Sayfaları HTML olarak CKEditor'de düzenler.
    /// </summary>
    public class NoteWindowViewModel : ReactiveObject
    {
        private readonly INoteService _noteService;

        public NoteWindowViewModel(INoteService noteService)
        {
            _noteService = noteService;
            Pages = new ObservableCollection<PageViewModel>();

            AddPageCommand = ReactiveCommand.CreateFromTask(OnAddPageAsync);
            SaveCommand    = ReactiveCommand.CreateFromTask(SaveAllPagesAsync);
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
                Pages.Add(new PageViewModel(page.Id, page.PageIndex, page.Content, page.IsReadOnly));
            }

            SelectedPage = Pages.FirstOrDefault();
        }

        public ReactiveCommand<Unit, Unit> AddPageCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        private async Task OnAddPageAsync()
        {
            if (Note is null) return;
            await _noteService.AddPageToNoteAsync(Note.Id);
            await LoadNoteAsync(Note.Id);
        }

        public async Task SaveAllPagesAsync()
        {
            if (Note is null) return;

            foreach (var pageVm in Pages)
            {
                var pageEntity = Note.Pages.FirstOrDefault(p => p.Id == pageVm.PageId);
                if (pageEntity != null)
                {
                    pageEntity.Content = pageVm.Html ?? string.Empty;
                }
            }

            await _noteService.UpdateNoteAsync(Note);
        }
    }

    /// <summary>
    /// CKEditor'de düzenlenen bir not sayfası.
    /// </summary>
    public class PageViewModel : ReactiveObject
    {
        public Guid PageId { get; }
        public int PageIndex { get; }
        public string Header => $"Sayfa {PageIndex}";

        private string? _html;
        public string? Html
        {
            get => _html;
            set => this.RaiseAndSetIfChanged(ref _html, value);
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
        }

        public PageViewModel(Guid id, int index, string html, bool isReadOnly)
        {
            PageId = id;
            PageIndex = index;
            _html = html;
            _isReadOnly = isReadOnly;
        }
    }
}

