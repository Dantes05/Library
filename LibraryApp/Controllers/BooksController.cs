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

        [Authorize(Policy = "AuthenticatedUsers")]
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



        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Файл не предоставлен.");
                }

                var book = await _bookRepository.GetByIdAsync(id);
                if (book == null)
                {
                    return NotFound("Книга не найдена.");
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine("wwwroot/images", fileName);

                Console.WriteLine($"Начинаем сохранение изображения: {filePath}");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                book.ImagePath = $"/images/{fileName}";
                await _bookRepository.UpdateAsync(book);

                Console.WriteLine($"Изображение сохранено: {book.ImagePath}");

                return Ok(new { ImagePath = book.ImagePath });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке изображения: {ex.Message}");
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromForm] BookCreateDto bookDto)
        {
            try
            {
                if (bookDto == null)
                {
                    return BadRequest("Invalid book data.");
                }

                if (bookDto.AuthorId == 0)
                {
                    return BadRequest("AuthorId is required.");
                }

                var existingAuthor = await _authorRepository.GetByIdAsync(bookDto.AuthorId);
                if (existingAuthor == null)
                {
                    return BadRequest("Author not found.");
                }

                var book = new Book
                {
                    ISBN = bookDto.ISBN,
                    Title = bookDto.Title,
                    Genre = bookDto.Genre,
                    Description = bookDto.Description,
                    AuthorId = bookDto.AuthorId
                };

                if (bookDto.Image != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(bookDto.Image.FileName);
                    var filePath = Path.Combine("wwwroot/images", fileName);

                    Console.WriteLine($"📌 Начинаем сохранение изображения: {filePath}");

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await bookDto.Image.CopyToAsync(stream);
                    }

                    book.ImagePath = $"/images/{fileName}";
                }

                await _bookRepository.AddAsync(book);

                Console.WriteLine($"Книга добавлена: {book.Title}");

                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении книги: {ex.Message}");
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookUpdateDto bookDto)
        {
            if (id != bookDto.Id)
            {
                return BadRequest();
            }

            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            book.ISBN = bookDto.ISBN;
            book.Title = bookDto.Title;
            book.Genre = bookDto.Genre;
            book.Description = bookDto.Description;
            book.AuthorId = bookDto.AuthorId;

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
                    await bookDto.Image.CopyToAsync(stream);
                }

                book.ImagePath = $"/images/{fileName}";
            }

            await _bookRepository.UpdateAsync(book);

            return NoContent();
        }


        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();
            await _bookRepository.DeleteAsync(book);
            return NoContent();
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{bookId}/rentals")]
        public async Task<IActionResult> GetRentalsByBookId(int bookId)
        {
            var rentals = await _bookRentalRepository.GetRentalsByBookId(bookId);
            return Ok(rentals);
        }

        [Authorize(Policy = "AuthenticatedUsers")]
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

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpPost("return/{bookId}")]
        public async Task<IActionResult> ReturnBook(int bookId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            Console.WriteLine($"Проверяем аренду: bookId={bookId}, userId={userId}");

            var rental = await _bookRentalRepository.GetActiveRental(bookId, userId);

            if (rental == null)
            {
                Console.WriteLine($"Аренда не найдена: bookId={bookId}, userId={userId}");
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
        [Authorize(Policy = "AuthenticatedUsers")]
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
        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("user/rentals/notifications")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Пользователь не авторизован.");
            }

            var notifications = new List<object>();
            var currentDate = DateTime.UtcNow;

            var rentals = await _context.BookRentals
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    r.BookId,
                    r.ReturnedAt
                })
                .ToListAsync();

            if (!rentals.Any())
            {
                return Ok(new { Message = "Нет активных аренд.", Type = "info" });
            }

            var bookIds = rentals.Select(r => r.BookId).Distinct().ToList();
            var books = await _context.Books
                .Where(b => bookIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id, b => b.Title); 

            foreach (var rental in rentals)
            {
                if (!rental.ReturnedAt.HasValue) continue;

                var returnDate = rental.ReturnedAt.Value;
                books.TryGetValue(rental.BookId, out var bookTitle); 

                if (bookTitle == null)
                {
                    Console.WriteLine($"Ошибка: Книга с ID {rental.BookId} не найдена!");
                    continue;
                }

                if (returnDate < currentDate)
                {
                    notifications.Add(new
                    {
                        Message = $"Срок сдачи книги \"{bookTitle}\" ПРОСРОЧЕН!",
                        Type = "error"
                    });
                }
                else if ((returnDate - currentDate).TotalHours <= 24)
                {
                    notifications.Add(new
                    {
                        Message = $"Срок сдачи книги \"{bookTitle}\" истекает завтра!",
                        Type = "warning"
                    });
                }
            }

            return Ok(notifications);
        }


        [Authorize(Policy = "AuthenticatedUsers")]
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
