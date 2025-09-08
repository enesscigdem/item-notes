using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using ItemNotes.Application.Interfaces;
using ItemNotes.Domain.Enums;
using ReactiveUI;

namespace ItemNotes.UI.ViewModels
{
    /// <summary>Yeni not oluşturma penceresi için VM.</summary>
    public class CreateNoteWindowViewModel : ReactiveObject
    {
        private readonly INoteService _noteService;

        public CreateNoteWindowViewModel(INoteService noteService)
        {
            _noteService = noteService;

            // ComboBox listesi (enum sabitleri)
            Colors = new ObservableCollection<NoteColor>
            {
                NoteColor.Yellow,
                NoteColor.Green,
                NoteColor.Red
            };

            SelectedColor = NoteColor.Yellow;
            CreatedBy = Environment.UserName;

            CreateCommand = ReactiveCommand.CreateFromTask(OnCreateAsync);
            CancelCommand = ReactiveCommand.Create(() => CloseRequested?.Invoke(null));
        }

        // --- Form alanları ---
        public ObservableCollection<NoteColor> Colors { get; }

        private NoteColor _selectedColor;
        public NoteColor SelectedColor
        {
            get => _selectedColor;
            set => this.RaiseAndSetIfChanged(ref _selectedColor, value);
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private string _createdBy = string.Empty;
        public string CreatedBy
        {
            get => _createdBy;
            set => this.RaiseAndSetIfChanged(ref _createdBy, value);
        }

        // --- Komutlar ---
        public ReactiveCommand<Unit, Unit> CreateCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private async Task OnCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Title)) return;

            var note = await _noteService.CreateNoteAsync(Title, SelectedColor, CreatedBy);
            // Pencereyi kapat, sonucu (oluşan notun Id'si) geri ver
            CloseRequested?.Invoke(note.Id);
        }

        public event Action<Guid?>? CloseRequested;
    }
}
