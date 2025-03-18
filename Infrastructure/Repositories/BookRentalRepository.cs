using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class BookRentalRepository : IBookRentalRepository
    {
        private readonly ApplicationDbContext _context;

        public BookRentalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BookRental?> GetActiveRental(int bookId, string userId)
        {
            return await _context.BookRentals
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId && r.ReturnedAt == null);
        }

        public async Task AddRental(BookRental rental)
        {
            await _context.BookRentals.AddAsync(rental);
            await _context.SaveChangesAsync();
        }

        public async Task CompleteRental(BookRental rental)
        {
            rental.ReturnedAt = DateTime.UtcNow;
            _context.BookRentals.Update(rental);
            await _context.SaveChangesAsync();
        }
    }
}
