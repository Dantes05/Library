using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<Book?> GetByISBNAsync(string isbn,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> GetBooksByAuthorIdAsync(int authorId,
            CancellationToken cancellationToken = default);
        Task<IEnumerable<Book>> GetFilteredBooksAsync(
            string? searchTerm,
            string? genre,
            int? authorId,
            CancellationToken cancellationToken = default);
    }
}