using Application.Interfaces;
using Domain.Entities;
using LibraryApp.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryApp.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRentalRepository _bookRentalRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository; // Добавили репозиторий авторов

        public BooksController(IBookRentalRepository bookRentalRepository, IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _bookRentalRepository = bookRentalRepository;
            _bookRepository = bookRepository;
            _authorRepository = authorRepository; // Инициализируем _authorRepository
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookRepository.GetAllAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (id != book.Id) return BadRequest();
            await _bookRepository.UpdateAsync(book);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();
            await _bookRepository.DeleteAsync(book);
            return NoContent();
        }

        [Authorize]
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] BookBorrowRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var book = await _bookRepository.GetByIdAsync(request.BookId);
            if (book == null)
                return NotFound("Book not found.");

            var activeRental = await _bookRentalRepository.GetActiveRental(request.BookId, userId);
            if (activeRental != null)
                return BadRequest("You have already borrowed this book.");

            var rental = new BookRental
            {
                BookId = request.BookId,
                UserId = userId
            };

            await _bookRentalRepository.AddRental(rental);

            return Ok("Book successfully borrowed.");
        }

        [Authorize]
        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromBody] BookReturnRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var rental = await _bookRentalRepository.GetActiveRental(request.BookId, userId);
            if (rental == null)
                return BadRequest("You haven't borrowed this book.");

            await _bookRentalRepository.CompleteRental(rental);

            return Ok("Book successfully returned.");
        }

    }
}
