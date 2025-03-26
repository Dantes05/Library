using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IBookRentalRepository
    {
        Task<BookRental?> GetActiveRental(int bookId, string userId,
            CancellationToken cancellationToken = default);
        Task AddRental(BookRental rental,
            CancellationToken cancellationToken = default);
        Task CompleteRental(BookRental rental,
            CancellationToken cancellationToken = default);
        Task<List<BookRental>> GetRentalsByBookId(int bookId,
            CancellationToken cancellationToken = default);
        Task<List<BookRental>> GetRentalsByUserId(string userId,
            CancellationToken cancellationToken = default);
        Task<bool> IsBookRentedByUser(int bookId, string userId,
            CancellationToken cancellationToken = default);
        Task UpdateAsync(BookRental rental,
            CancellationToken cancellationToken = default);
        Task DeleteRental(BookRental rental,
            CancellationToken cancellationToken = default);
    }
}