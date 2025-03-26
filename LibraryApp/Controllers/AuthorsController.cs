using Application.DTOs;
using Application.Services;
using Application.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryApp.Controllers
{
    [AllowAnonymous]
    [Route("api/authors")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly AuthorService _authorService;

        public AuthorsController(AuthorService authorService)
        {
            _authorService = authorService;
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet]
        public async Task<IActionResult> GetAllAuthors(CancellationToken cancellationToken)
        {
            var authors = await _authorService.GetAllAuthorsAsync(cancellationToken);
            return Ok(authors);
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthorById(int id, CancellationToken cancellationToken)
        {
            var author = await _authorService.GetAuthorByIdAsync(id, cancellationToken);
            return Ok(author);
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{id}/books")]
        public async Task<IActionResult> GetBooksByAuthor(int id, CancellationToken cancellationToken)
        {
            var books = await _authorService.GetBooksByAuthorIdAsync(id, cancellationToken);
            return Ok(books);
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPost]
        public async Task<IActionResult> CreateAuthor(
            [FromBody] CreateAuthorDto createAuthorDto,
            CancellationToken cancellationToken)
        {
            var createdAuthor = await _authorService.CreateAuthorAsync(createAuthorDto, cancellationToken);
            return CreatedAtAction(
                nameof(GetAuthorById),
                new { id = createdAuthor.Id },
                createdAuthor
            );
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(
            int id,
            [FromBody] AuthorDto authorDto,
            CancellationToken cancellationToken)
        {
            await _authorService.UpdateAuthorAsync(id, authorDto, cancellationToken);
            return NoContent();
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(
            int id,
            CancellationToken cancellationToken)
        {
            await _authorService.DeleteAuthorAsync(id, cancellationToken);
            return NoContent();
        }
    }
}