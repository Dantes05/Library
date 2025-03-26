using Domain.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
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

        public async Task<List<BookRental>> GetRentalsByBookId(
            int bookId,
            CancellationToken cancellationToken = default)
        {
            return await _context.BookRentals
                .Where(r => r.BookId == bookId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<BookRental>> GetRentalsByUserId(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.BookRentals
                .Where(r => r.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsBookRentedByUser(
            int bookId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.BookRentals
                .AnyAsync(r => r.BookId == bookId && r.UserId == userId,
                    cancellationToken);
        }

        public async Task<BookRental?> GetActiveRental(
            int bookId,
            string userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.BookRentals
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId,
                    cancellationToken);
        }

        public async Task DeleteRental(
            BookRental rental,
            CancellationToken cancellationToken = default)
        {
            _context.BookRentals.Remove(rental);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task AddRental(
            BookRental rental,
            CancellationToken cancellationToken = default)
        {
            await _context.BookRentals.AddAsync(rental, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task CompleteRental(
            BookRental rental,
            CancellationToken cancellationToken = default)
        {
            rental.ReturnedAt = DateTime.UtcNow;
            _context.BookRentals.Update(rental);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(
            BookRental rental,
            CancellationToken cancellationToken = default)
        {
            _context.BookRentals.Update(rental);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}