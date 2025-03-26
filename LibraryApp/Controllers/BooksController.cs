using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs;

namespace LibraryApp.Controllers
{
    [AllowAnonymous]
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;

        public BooksController(BookService bookService)
        {
            _bookService = bookService;
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet]
        public async Task<IActionResult> GetAllBooks(
            [FromQuery] string? search,
            [FromQuery] string? genre,
            [FromQuery] string? author,
            CancellationToken cancellationToken)
        {
            var books = await _bookService.GetAllBooksAsync(search, genre, author, cancellationToken);
            return Ok(books);
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> UploadImage(
            int id,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var imagePath = await _bookService.UploadImageAsync(id, file, cancellationToken);
            return Ok(new { ImagePath = imagePath });
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(
            int id,
            CancellationToken cancellationToken)
        {
            var book = await _bookService.GetBookByIdAsync(id, cancellationToken);
            return Ok(book);
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPost]
        public async Task<IActionResult> CreateBook(
            [FromForm] BookCreateDto bookDto,
            CancellationToken cancellationToken)
        {
            var book = await _bookService.CreateBookAsync(bookDto, cancellationToken);
            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(
            int id,
            [FromForm] BookUpdateDto bookDto,
            CancellationToken cancellationToken)
        {
            await _bookService.UpdateBookAsync(id, bookDto, cancellationToken);
            return NoContent();
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(
            int id,
            CancellationToken cancellationToken)
        {
            await _bookService.DeleteBookAsync(id, cancellationToken);
            return NoContent();
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{bookId}/rentals")]
        public async Task<IActionResult> GetRentalsByBookId(
            int bookId,
            CancellationToken cancellationToken)
        {
            var rentals = await _bookService.GetRentalsByBookIdAsync(bookId, cancellationToken);
            return Ok(rentals);
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook(
            [FromBody] BookBorrowRequest request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _bookService.BorrowBookAsync(userId, request, cancellationToken);
            return Ok("Книга успешно взята.");
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpPost("return/{bookId}")]
        public async Task<IActionResult> ReturnBook(
            int bookId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _bookService.ReturnBookAsync(userId, bookId, cancellationToken);
            return Ok("Книга успешно возвращена.");
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("user/rentals")]
        public async Task<IActionResult> GetUserRentals(
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rentals = await _bookService.GetUserRentalsAsync(userId, cancellationToken);
            return Ok(rentals);
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("user/rentals/notifications")]
        public async Task<IActionResult> GetUserNotifications(
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var notifications = await _bookService.GetUserNotificationsAsync(userId, cancellationToken);
            return Ok(notifications);
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{bookId}/is-rented")]
        public async Task<IActionResult> IsBookRentedByUser(
            int bookId,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isRented = await _bookService.IsBookRentedByUserAsync(bookId, userId, cancellationToken);
            return Ok(new { isRented });
        }
    }
}