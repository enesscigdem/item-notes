using ItemNotes.Application.Interfaces;
using ItemNotes.Domain.Entities;
using ItemNotes.Domain.Enums;
using ItemNotes.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemNotes.Application.Services
{
    /// <summary>
    /// Notlarla ilgili iş kuralları ve operasyonları.
    /// </summary>
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;

        public NoteService(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public async Task<IList<Note>> GetAllNotesAsync()
        {
            return await _noteRepository.GetAllAsync();
        }

        public async Task<Note?> GetNoteAsync(Guid id)
        {
            return await _noteRepository.GetByIdAsync(id);
        }

        public async Task<Note> CreateNoteAsync(string title, NoteColor color, string createdBy)
        {
            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = title,
                Color = color,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                Pages = new List<NotePage>()
            };

            // Yeni not içinde başlangıç sayfası oluştur
            note.Pages.Add(new NotePage
            {
                Id = Guid.NewGuid(),
                NoteId = note.Id,
                PageIndex = 1,
                Content = string.Empty,
                IsReadOnly = false
            });

            await _noteRepository.AddAsync(note);
            await _noteRepository.SaveChangesAsync();
            return note;
        }

        public async Task AddPageToNoteAsync(Guid noteId)
        {
            var note = await _noteRepository.GetByIdAsync(noteId);
            if (note == null) return;
            var maxIndex = note.Pages.Any() ? note.Pages.Max(p => p.PageIndex) : 0;
            note.Pages.Add(new NotePage
            {
                Id = Guid.NewGuid(),
                NoteId = noteId,
                PageIndex = maxIndex + 1,
                Content = string.Empty,
                IsReadOnly = false
            });
            await _noteRepository.UpdateAsync(note);
            await _noteRepository.SaveChangesAsync();
        }

        public async Task UpdateNoteAsync(Note note)
        {
            await _noteRepository.UpdateAsync(note);
            await _noteRepository.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(Guid noteId)
        {
            await _noteRepository.DeleteAsync(noteId);
            await _noteRepository.SaveChangesAsync();
        }
    }
}