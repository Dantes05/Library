using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;
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
        public async Task<IActionResult> GetAllBooks([FromQuery] string? search, [FromQuery] string? genre, [FromQuery] string? author)
        {
            var books = await _bookService.GetAllBooksAsync(search, genre, author);
            return Ok(books);
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

                var imagePath = await _bookService.UploadImageAsync(id, file);
                return Ok(new { ImagePath = imagePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromForm] BookCreateDto bookDto)
        {
            try
            {
                var book = await _bookService.CreateBookAsync(bookDto);
                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookUpdateDto bookDto)
        {
            try
            {
                await _bookService.UpdateBookAsync(id, bookDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                await _bookService.DeleteBookAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{bookId}/rentals")]
        public async Task<IActionResult> GetRentalsByBookId(int bookId)
        {
            var rentals = await _bookService.GetRentalsByBookIdAsync(bookId);
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

            try
            {
                await _bookService.BorrowBookAsync(userId, request);
                return Ok("Книга успешно взята.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
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

            try
            {
                await _bookService.ReturnBookAsync(userId, bookId);
                return Ok("Книга успешно возвращена.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
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

            var rentals = await _bookService.GetUserRentalsAsync(userId);
            return Ok(rentals);
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

            var notifications = await _bookService.GetUserNotificationsAsync(userId);
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

            var isRented = await _bookService.IsBookRentedByUserAsync(bookId, userId);
            return Ok(new { isRented });
        }
    }
}