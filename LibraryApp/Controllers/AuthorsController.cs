using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApp.Controllers
{
    [Authorize]
    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository;

        public AuthorsController(IAuthorRepository authorRepository, IBookRepository bookRepository)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllAuthors()
        {
            var authors = await _authorRepository.GetAllAsync();
            return Ok(authors);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthorById(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null) return NotFound();
            return Ok(author);
        }

        [AllowAnonymous]
        [HttpGet("{id}/books")]
        public async Task<IActionResult> GetBooksByAuthor(int id)
        {
            var books = await _bookRepository.GetBooksByAuthorIdAsync(id);
            return Ok(books);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAuthor([FromBody] Author author)
        {
            await _authorRepository.AddAsync(author);
            return CreatedAtAction(nameof(GetAuthorById), new { id = author.Id }, author);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, [FromBody] Author author)
        {
            if (id != author.Id) return BadRequest();
            await _authorRepository.UpdateAsync(author);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null) return NotFound();
            await _authorRepository.DeleteAsync(author);
            return NoContent();
        }
    }
}
