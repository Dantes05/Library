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
    }
}
