using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class BookService
{
    private readonly IBookRentalRepository _bookRentalRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public BookService(
        IBookRentalRepository bookRentalRepository,
        IBookRepository bookRepository,
        IAuthorRepository authorRepository,
        ApplicationDbContext context,
        IMapper mapper)
    {
        _bookRentalRepository = bookRentalRepository;
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync(
        string? search,
        string? genre,
        string? author,
        CancellationToken cancellationToken = default)
    {
        int? authorId = null;
        if (!string.IsNullOrWhiteSpace(author))
        {
            var authorEntity = await _authorRepository.GetByNameAsync(author, cancellationToken);
            if (authorEntity == null)
            {
                return Enumerable.Empty<BookDto>();
            }
            authorId = authorEntity.Id;
        }

        var books = await _bookRepository.GetFilteredBooksAsync(search, genre, authorId, cancellationToken);
        var authorIds = books.Select(b => b.AuthorId).Distinct();
        var authors = await _authorRepository.FindAsync(a => authorIds.Contains(a.Id), cancellationToken);

        var authorDictionary = authors.ToDictionary(a => a.Id, a => $"{a.FirstName} {a.LastName}");

        return books.Select(book => new BookDto
        {
            Id = book.Id,
            ISBN = book.ISBN,
            Title = book.Title,
            Genre = book.Genre,
            Description = book.Description,
            AuthorId = book.AuthorId,
            TakenAt = book.TakenAt,
            ReturnAt = book.ReturnAt,
            AuthorName = authorDictionary.TryGetValue(book.AuthorId, out var name) ? name : "Unknown",
            ImagePath = book.ImagePath
        });
    }

    public async Task<string> UploadImageAsync(
        int id,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Файл не предоставлен.");
        }

        var book = await _bookRepository.GetByIdAsync(id, cancellationToken) ??
            throw new KeyNotFoundException("Книга не найдена.");

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine("wwwroot/images", fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        book.ImagePath = $"/images/{fileName}";
        await _bookRepository.UpdateAsync(book, cancellationToken);

        return book.ImagePath;
    }

    public async Task<BookDto> GetBookByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken) ??
            throw new KeyNotFoundException("Book not found");
        var author = await _authorRepository.GetByIdAsync(book.AuthorId, cancellationToken);

        return new BookDto
        {
            Id = book.Id,
            ISBN = book.ISBN,
            Title = book.Title,
            Genre = book.Genre,
            Description = book.Description,
            AuthorId = book.AuthorId,
            TakenAt = book.TakenAt,
            ReturnAt = book.ReturnAt,
            AuthorName = $"{author?.FirstName} {author?.LastName}",
            ImagePath = book.ImagePath
        };
    }

    public async Task<BookDto> CreateBookAsync(
        BookCreateDto bookDto,
        CancellationToken cancellationToken = default)
    {
        if (bookDto == null)
        {
            throw new ArgumentNullException(nameof(bookDto));
        }

        if (bookDto.AuthorId == 0)
        {
            throw new ArgumentException("AuthorId is required.");
        }

        var existingAuthor = await _authorRepository.GetByIdAsync(bookDto.AuthorId, cancellationToken) ??
            throw new ArgumentException("Author not found.");

        var book = _mapper.Map<Book>(bookDto);

        if (bookDto.Image != null)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(bookDto.Image.FileName);
            var filePath = Path.Combine("wwwroot/images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bookDto.Image.CopyToAsync(stream, cancellationToken);
            }

            book.ImagePath = $"/images/{fileName}";
        }

        await _bookRepository.AddAsync(book, cancellationToken);

        return _mapper.Map<BookDto>(book);
    }

    public async Task UpdateBookAsync(
        int id,
        BookUpdateDto bookDto,
        CancellationToken cancellationToken = default)
    {
        if (id != bookDto.Id)
        {
            throw new ArgumentException("ID mismatch");
        }

        var book = await _bookRepository.GetByIdAsync(id, cancellationToken) ??
            throw new KeyNotFoundException("Book not found");

        _mapper.Map(bookDto, book);

        if (bookDto.Image != null)
        {
            if (!string.IsNullOrEmpty(book.ImagePath))
            {
                var oldFilePath = Path.Combine("wwwroot", book.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(bookDto.Image.FileName);
            var filePath = Path.Combine("wwwroot/images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await bookDto.Image.CopyToAsync(stream, cancellationToken);
            }

            book.ImagePath = $"/images/{fileName}";
        }

        await _bookRepository.UpdateAsync(book, cancellationToken);
    }

    public async Task DeleteBookAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken) ??
            throw new KeyNotFoundException("Book not found");
        await _bookRepository.DeleteAsync(book, cancellationToken);
    }

    public async Task<IEnumerable<BookRentalDto>> GetRentalsByBookIdAsync(
        int bookId,
        CancellationToken cancellationToken = default)
    {
        var rentals = await _bookRentalRepository.GetRentalsByBookId(bookId, cancellationToken);
        return _mapper.Map<IEnumerable<BookRentalDto>>(rentals);
    }

    public async Task BorrowBookAsync(
        string userId,
        BookBorrowRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID is required");
        }

        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken) ??
            throw new KeyNotFoundException("Book not found");

        var rental = new BookRental
        {
            BookId = request.BookId,
            UserId = userId,
            BorrowedAt = DateTime.UtcNow,
            ReturnedAt = request.ReturnAt
        };

        await _bookRentalRepository.AddRental(rental, cancellationToken);

        book.TakenAt = DateTime.UtcNow;
        book.ReturnAt = request.ReturnAt;
        await _bookRepository.UpdateAsync(book, cancellationToken);
    }

    public async Task ReturnBookAsync(
        string userId,
        int bookId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID is required");
        }

        var rental = await _bookRentalRepository.GetActiveRental(bookId, userId, cancellationToken) ??
            throw new KeyNotFoundException("Rental not found");

        await _bookRentalRepository.DeleteRental(rental, cancellationToken);

        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken);
        if (book != null)
        {
            book.TakenAt = null;
            book.ReturnAt = null;
            await _bookRepository.UpdateAsync(book, cancellationToken);
        }
    }

    public async Task<IEnumerable<BookRentalDto>> GetUserRentalsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID is required");
        }

        var rentals = await _bookRentalRepository.GetRentalsByUserId(userId, cancellationToken);

        var rentalDtos = new List<BookRentalDto>();
        foreach (var rental in rentals)
        {
            var book = await _bookRepository.GetByIdAsync(rental.BookId, cancellationToken);
            if (book != null)
            {
                var author = await _authorRepository.GetByIdAsync(book.AuthorId, cancellationToken);
                rentalDtos.Add(new BookRentalDto
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Genre = book.Genre,
                    Description = book.Description,
                    AuthorName = $"{author?.FirstName} {author?.LastName}",
                    BorrowedAt = rental.BorrowedAt,
                    ReturnedAt = rental.ReturnedAt
                });
            }
        }

        return rentalDtos;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID is required");
        }

        var notifications = new List<NotificationDto>();
        var currentDate = DateTime.UtcNow;

        var rentals = await _context.BookRentals
            .Where(r => r.UserId == userId)
            .Select(r => new
            {
                r.BookId,
                r.ReturnedAt
            })
            .ToListAsync(cancellationToken);

        if (!rentals.Any())
        {
            return new List<NotificationDto> { new NotificationDto { Message = "Нет активных аренд.", Type = "info" } };
        }

        var bookIds = rentals.Select(r => r.BookId).Distinct().ToList();
        var books = await _context.Books
            .Where(b => bookIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.Title, cancellationToken);

        foreach (var rental in rentals)
        {
            if (!rental.ReturnedAt.HasValue) continue;

            var returnDate = rental.ReturnedAt.Value;
            if (!books.TryGetValue(rental.BookId, out var bookTitle)) continue;

            if (returnDate < currentDate)
            {
                notifications.Add(new NotificationDto
                {
                    Message = $"Срок сдачи книги \"{bookTitle}\" ПРОСРОЧЕН!",
                    Type = "error"
                });
            }
            else if ((returnDate - currentDate).TotalHours > 0 || (returnDate - currentDate).TotalHours < 24)
            {
                notifications.Add(new NotificationDto
                {
                    Message = $"Срок сдачи книги \"{bookTitle}\" скоро истекает!",
                    Type = "warning"
                });
            }
        }

        return notifications;
    }

    public async Task<bool> IsBookRentedByUserAsync(
        int bookId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID is required");
        }

        return await _bookRentalRepository.IsBookRentedByUser(bookId, userId, cancellationToken);
    }
}