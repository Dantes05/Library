using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Extensions;

namespace Application.Services
{
    public class AuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public AuthorService(
            IAuthorRepository authorRepository,
            IBookRepository bookRepository,
            IMapper mapper)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync(CancellationToken cancellationToken = default)
        {
            var authors = await _authorRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<AuthorDto>>(authors);
        }

        public async Task<AuthorDto> GetAuthorByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var author = await _authorRepository.GetByIdAsync(id, cancellationToken);
            if (author == null)
            {
                throw new NotFoundException($"Author with ID {id} not found.");
            }
            return _mapper.Map<AuthorDto>(author);
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorIdAsync(int authorId, CancellationToken cancellationToken = default)
        {
            var author = await _authorRepository.GetByIdAsync(authorId, cancellationToken);
            if (author == null)
            {
                throw new NotFoundException($"Author with ID {authorId} not found.");
            }

            var books = await _bookRepository.GetBooksByAuthorIdAsync(authorId, cancellationToken);
            if (books == null || !books.Any())
            {
                throw new NotFoundException($"No books found for author with ID {authorId}.");
            }

            return books;
        }

        public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto createAuthorDto, CancellationToken cancellationToken = default)
        {
            if (createAuthorDto == null)
            {
                throw new ArgumentNullException(nameof(createAuthorDto));
            }

            var author = _mapper.Map<Author>(createAuthorDto);
            await _authorRepository.AddAsync(author, cancellationToken);
            return _mapper.Map<AuthorDto>(author);
        }

        public async Task UpdateAuthorAsync(int id, AuthorDto authorDto, CancellationToken cancellationToken = default)
        {
            if (authorDto == null)
            {
                throw new ArgumentNullException(nameof(authorDto));
            }

            if (id != authorDto.Id)
            {
                throw new ValidationException("ID in URL does not match the author's ID.");
            }

            var existingAuthor = await _authorRepository.GetByIdAsync(id, cancellationToken);
            if (existingAuthor == null)
            {
                throw new NotFoundException($"Author with ID {id} not found.");
            }

            _mapper.Map(authorDto, existingAuthor);
            await _authorRepository.UpdateAsync(existingAuthor, cancellationToken);
        }

        public async Task DeleteAuthorAsync(int id, CancellationToken cancellationToken = default)
        {
            var author = await _authorRepository.GetByIdAsync(id, cancellationToken);
            if (author == null)
            {
                throw new NotFoundException($"Author with ID {id} not found.");
            }

            await _authorRepository.DeleteAsync(author, cancellationToken);
        }
    }
}