using ItemNotes.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ItemNotes.Infrastructure.Repositories
{
    /// <summary>
    /// Not verileri üzerinde temel CRUD işlemlerini tanımlar.
    /// </summary>
    public interface INoteRepository
    {
        Task<Note?> GetByIdAsync(Guid id);
        Task<IList<Note>> GetAllAsync();
        Task AddAsync(Note note);
        Task UpdateAsync(Note note);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}