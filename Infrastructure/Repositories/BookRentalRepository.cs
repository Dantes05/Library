using Domain.Interfaces;
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
        public async Task<List<BookRental>> GetRentalsByBookId(int bookId)
        {
            return await _context.BookRentals
                .Where(r => r.BookId == bookId)
                .ToListAsync();
        }
        public async Task<List<BookRental>> GetRentalsByUserId(string userId)
        {
            return await _context.BookRentals
                .Where(r => r.UserId == userId)
                .ToListAsync();
        }
        public async Task<bool> IsBookRentedByUser(int bookId, string userId)
        {
            return await _context.BookRentals
                .AnyAsync(r => r.BookId == bookId && r.UserId == userId);
        }
        public async Task<BookRental?> GetActiveRental(int bookId, string userId)
        {
            return await _context.BookRentals
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId);
        }
        public async Task DeleteRental(BookRental rental)
        {
            _context.BookRentals.Remove(rental);
            await _context.SaveChangesAsync();
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
        public async Task UpdateAsync(BookRental rental)
        {
            _context.BookRentals.Update(rental);
            await _context.SaveChangesAsync();
        }
    }
}
