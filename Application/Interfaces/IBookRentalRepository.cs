using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBookRentalRepository
    {
        Task<BookRental?> GetActiveRental(int bookId, string userId);
        Task AddRental(BookRental rental);
        Task CompleteRental(BookRental rental);
        Task<List<BookRental>> GetRentalsByBookId(int bookId);
        Task<List<BookRental>> GetRentalsByUserId(string userId);
        Task<bool> IsBookRentedByUser(int bookId, string userId);
        Task UpdateAsync(BookRental rental);
        Task DeleteRental(BookRental rental);
    }
}
