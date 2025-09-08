using ItemNotes.Domain.Entities;
using ItemNotes.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItemNotes.Application.Interfaces
{
    /// <summary>
    /// Not işlemleri için servis arayüzü.
    /// </summary>
    public interface INoteService
    {
        /// <summary>
        /// Tüm notları getirir.
        /// </summary>
        Task<IList<Note>> GetAllNotesAsync();

        /// <summary>
        /// Verilen kimliğe sahip notu getirir.
        /// </summary>
        Task<Note?> GetNoteAsync(Guid id);

        /// <summary>
        /// Yeni bir not oluşturur.
        /// </summary>
        Task<Note> CreateNoteAsync(string title, NoteColor color, string createdBy);

        /// <summary>
        /// Mevcut nota yeni bir sayfa ekler.
        /// </summary>
        Task AddPageToNoteAsync(Guid noteId);

        /// <summary>
        /// Notu ve sayfalarını günceller.
        /// </summary>
        Task UpdateNoteAsync(Note note);

        /// <summary>
        /// Notu siler.
        /// </summary>
        Task DeleteNoteAsync(Guid noteId);
    }
}