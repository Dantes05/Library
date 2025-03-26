using Domain.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Book?> GetByISBNAsync(
            string isbn,
            CancellationToken cancellationToken = default)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.ISBN == isbn, cancellationToken);
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorIdAsync(
            int authorId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Books
                .Where(b => b.AuthorId == authorId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Book>> GetFilteredBooksAsync(
            string? searchTerm,
            string? genre,
            int? authorId,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => b.Title.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                query = query.Where(b => b.Genre == genre);
            }

            if (authorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == authorId.Value);
            }

            return await query.ToListAsync(cancellationToken);
        }
    }
}