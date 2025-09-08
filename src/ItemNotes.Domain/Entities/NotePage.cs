using System;
using ItemNotes.Domain.Entities;

namespace ItemNotes.Domain.Entities
{
    /// <summary>
    /// Bir notun sayfası. İçerik metin, resim veya diğer öğelerden oluşan akış belgesidir.
    /// </summary>
    public class NotePage
    {
        /// <summary>
        /// Birincil anahtar.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// İlişkili notun anahtarı.
        /// </summary>
        public Guid NoteId { get; set; }

        /// <summary>
        /// Not içinde sayfanın sıralaması (1'den başlar).
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Sayfanın içerik verisi (RTF/XAML/plain text) olarak saklanır.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Sayfa düzenleme için kilitli mi?
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Navigasyon amacıyla ilişkili not.
        /// </summary>
        public Note Note { get; set; } = null!;
    }
}