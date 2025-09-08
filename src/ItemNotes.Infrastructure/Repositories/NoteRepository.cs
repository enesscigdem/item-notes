using ItemNotes.Domain.Entities;
using ItemNotes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItemNotes.Infrastructure.Repositories
{
    /// <summary>
    /// Not veri erişim katmanı. Entity Framework üzerinden veritabanına erişim sağlar.
    /// </summary>
    public class NoteRepository : INoteRepository
    {
        private readonly ItemNotesDbContext _dbContext;

        public NoteRepository(ItemNotesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Note note)
        {
            await _dbContext.Notes.AddAsync(note);
        }

        public async Task DeleteAsync(Guid id)
        {
            var note = await _dbContext.Notes.FindAsync(id);
            if (note != null)
            {
                _dbContext.Notes.Remove(note);
            }
        }

        public async Task<IList<Note>> GetAllAsync()
        {
            return await _dbContext.Notes
                .Include(n => n.Pages)
                .ToListAsync();
        }

        public async Task<Note?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Notes
                .Include(n => n.Pages)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task UpdateAsync(Note note)
        {
            _dbContext.Notes.Update(note);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}