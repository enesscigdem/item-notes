using System;
using ItemNotes.Domain.Enums;

namespace ItemNotes.UI.Models
{
    /// <summary>
    /// Kullanıcı arayüzü için not modelinin temsilidir. Domain'deki Note nesnesini UI ile uyumlu hale getirir.
    /// </summary>
    public class NoteModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public NoteColor Color { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}