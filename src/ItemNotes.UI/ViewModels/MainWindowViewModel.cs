using ItemNotes.Application.Interfaces;
using ItemNotes.UI.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace ItemNotes.UI.ViewModels
{
    /// <summary>
    /// Ana pencerenin ViewModel'i. Not listesini yükler ve yeni not oluşturma / açma / silme komutlarını sağlar.
    /// </summary>
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly INoteService _noteService;

        public MainWindowViewModel(INoteService noteService)
        {
            _noteService = noteService;
            Notes = new ObservableCollection<NoteModel>();

            // Komutlar
            CreateNoteCommand = ReactiveCommand.CreateFromTask(OnCreateNoteAsync);
            OpenNoteCommand = ReactiveCommand.CreateFromTask(OnOpenNoteAsync, this.WhenAnyValue(vm => vm.SelectedNote).Select(note => note != null));
            DeleteNoteCommand = ReactiveCommand.CreateFromTask(OnDeleteNoteAsync, this.WhenAnyValue(vm => vm.SelectedNote).Select(note => note != null));
        }

        /// <summary>
        /// UI'da gösterilecek notların listesi.
        /// </summary>
        public ObservableCollection<NoteModel> Notes { get; }

        private NoteModel? _selectedNote;
        public NoteModel? SelectedNote
        {
            get => _selectedNote;
            set => this.RaiseAndSetIfChanged(ref _selectedNote, value);
        }

        public ReactiveCommand<Unit, Unit> CreateNoteCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenNoteCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteNoteCommand { get; }

        /// <summary>
        /// Veritabanındaki notları UI modeline dönüştürerek yükler.
        /// </summary>
        public async Task LoadNotesAsync()
        {
            Notes.Clear();
            var entities = await _noteService.GetAllNotesAsync();
            foreach (var note in entities.OrderByDescending(n => n.CreatedAt))
            {
                Notes.Add(new NoteModel
                {
                    Id = note.Id,
                    Title = note.Title,
                    Color = note.Color,
                    CreatedBy = note.CreatedBy,
                    CreatedAt = note.CreatedAt
                });
            }
        }

        private async Task OnCreateNoteAsync()
        {
            // Görünürde CreateNoteWindow gösterileceğini ana pencere code-behind'e bildir
            CreateNoteRequested?.Invoke();
        }

        private async Task OnOpenNoteAsync()
        {
            if (SelectedNote != null)
            {
                OpenNoteRequested?.Invoke(SelectedNote.Id);
            }
        }

        private async Task OnDeleteNoteAsync()
        {
            if (SelectedNote != null)
            {
                await _noteService.DeleteNoteAsync(SelectedNote.Id);
                await LoadNotesAsync();
            }
        }

        /// <summary>
        /// Yeni not oluşturma penceresi açılması gerektiğini haber veren olay.
        /// </summary>
        public event Action? CreateNoteRequested;

        /// <summary>
        /// Seçilen notun düzenleme penceresi açılması gerektiğini haber veren olay.
        /// </summary>
        public event Action<Guid>? OpenNoteRequested;
    }
}