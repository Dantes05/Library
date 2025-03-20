using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using LibraryApp.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryApp.Controllers
{
    [AllowAnonymous]
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRentalRepository _bookRentalRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context, IBookRentalRepository bookRentalRepository, IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _bookRentalRepository = bookRentalRepository;
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _context = context;
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetAllBooks([FromQuery] string? search, [FromQuery] string? genre, [FromQuery] string? author)
        {
            var booksQuery = _bookRepository.GetAllQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                booksQuery = booksQuery.Where(b => b.Title.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                booksQuery = booksQuery.Where(b => b.Genre == genre);
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                var authorEntity = await _authorRepository.GetByNameAsync(author);
                if (authorEntity != null)
                {
                    booksQuery = booksQuery.Where(b => b.AuthorId == authorEntity.Id);
                }
                else
                {
                    return Ok(new List<Book>());
                }
            }
           
            var booksWithAuthor = await booksQuery
                .Join(
                    _context.Authors,
                    book => book.AuthorId,
                    author => author.Id,
                    (book, author) => new 
                    {
                        Book = book,
                        AuthorName = $"{author.FirstName} {author.LastName}" 
                    }
                )
                .ToListAsync();

            var result = booksWithAuthor.Select(b => new
            {
                b.Book.Id,
                b.Book.ISBN,
                b.Book.Title,
                b.Book.Genre,
                b.Book.Description,
                b.Book.AuthorId,
                b.Book.TakenAt,
                b.Book.ReturnAt,
                AuthorName = b.AuthorName
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            if (book == null)
            {
                return BadRequest("Invalid book data.");
            }

            if (book.AuthorId == 0)
            {
                return BadRequest("AuthorId is required.");
            }

            var existingAuthor = await _authorRepository.GetByIdAsync(book.AuthorId);
            if (existingAuthor == null)
            {
                return BadRequest("Author not found. Please provide a valid authorId.");
            }

            await _bookRepository.AddAsync(book);

            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [AllowAnonymous]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (id != book.Id) return BadRequest();
            await _bookRepository.UpdateAsync(book);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();
            await _bookRepository.DeleteAsync(book);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("{bookId}/rentals")]
        public async Task<IActionResult> GetRentalsByBookId(int bookId)
        {
            var rentals = await _bookRentalRepository.GetRentalsByBookId(bookId);
            return Ok(rentals);
        }

        [Authorize]
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BookBorrowRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            var book = await _bookRepository.GetByIdAsync(request.BookId);
            if (book == null)
            {
                return NotFound("Книга не найдена.");
            }

            var rental = new BookRental
            {
                BookId = request.BookId,
                UserId = userId,
                BorrowedAt = DateTime.UtcNow,
                ReturnedAt = request.ReturnAt 
            };

            await _bookRentalRepository.AddRental(rental);

            
            book.TakenAt = DateTime.UtcNow;
            book.ReturnAt = request.ReturnAt;
            await _bookRepository.UpdateAsync(book);

            return Ok("Книга успешно взята.");
        }

        [Authorize]
        [HttpPost("return/{bookId}")]
        public async Task<IActionResult> ReturnBook(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            Console.WriteLine($"📌 Проверяем аренду: bookId={bookId}, userId={userId}");

            var rental = await _bookRentalRepository.GetActiveRental(bookId, userId);

            if (rental == null)
            {
                Console.WriteLine($"❌ Аренда не найдена: bookId={bookId}, userId={userId}");
                return NotFound("Аренда не найдена.");
            }

           
            await _bookRentalRepository.DeleteRental(rental);

           
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book != null)
            {
                book.TakenAt = null;
                book.ReturnAt = null;
                await _bookRepository.UpdateAsync(book);
            }

            return Ok("Книга успешно возвращена.");
        }
        [Authorize]
        [HttpGet("user/rentals")]
        public async Task<IActionResult> GetUserRentals()
        {
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            
            var rentals = await _bookRentalRepository.GetRentalsByUserId(userId);

            
            var books = new List<object>();
            foreach (var rental in rentals)
            {
                var book = await _bookRepository.GetByIdAsync(rental.BookId);
                if (book != null)
                {
                    var author = await _authorRepository.GetByIdAsync(book.AuthorId);
                    books.Add(new
                    {
                        Id = book.Id,
                        Title = book.Title,
                        Genre = book.Genre,
                        Description = book.Description,
                        AuthorName = $"{author?.FirstName} {author?.LastName}",
                        BorrowedAt = rental.BorrowedAt,
                        ReturnAt = rental.ReturnedAt
                    });
                }
            }

            return Ok(books);
        }

        [Authorize]
        [HttpGet("{bookId}/is-rented")]
        public async Task<IActionResult> IsBookRentedByUser(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            var isRented = await _bookRentalRepository.IsBookRentedByUser(bookId, userId);
            return Ok(new { isRented });
        }

    }
}
