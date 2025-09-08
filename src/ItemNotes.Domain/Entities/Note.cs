using System;
using System.Collections.Generic;
using ItemNotes.Domain.Enums;

namespace ItemNotes.Domain.Entities
{
    /// <summary>
    /// Temel not varlığı. Her not başlık, renk ve oluşturan kullanıcı bilgisi içerir.
    /// Bir not içerisinde birden fazla sayfa bulunabilir.
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Birincil anahtar.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Notun başlığı.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Notun rengi (kırmızı, yeşil, sarı).
        /// </summary>
        public NoteColor Color { get; set; }
        
        /// <summary>
        /// Notu oluşturan kullanıcı adı.
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Notun oluşturulma tarihi UTC olarak saklanır.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Notun sayfaları.
        /// </summary>
        public IList<NotePage> Pages { get; set; } = new List<NotePage>();
    }
}