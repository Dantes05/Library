// AuthorsController.cs
using Application.DTOs;
using Application.Services;
using Application.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
        public async Task<IActionResult> GetAllAuthors()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            return Ok(authors);
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthorById(int id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            return Ok(author);
        }

        [Authorize(Policy = "AuthenticatedUsers")]
        [HttpGet("{id}/books")]
        public async Task<IActionResult> GetBooksByAuthor(int id)
        {
            var books = await _authorService.GetBooksByAuthorIdAsync(id);
            return Ok(books);
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPost]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
        {
            var createdAuthor = await _authorService.CreateAuthorAsync(createAuthorDto);
            return CreatedAtAction(
                nameof(GetAuthorById),
                new { id = createdAuthor.Id },
                createdAuthor
            );
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, [FromBody] AuthorDto authorDto)
        {
            await _authorService.UpdateAuthorAsync(id, authorDto);
            return NoContent();
        }

        [Authorize(Policy = "OnlyAdminUsers")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            await _authorService.DeleteAuthorAsync(id);
            return NoContent();
        }
    }
}