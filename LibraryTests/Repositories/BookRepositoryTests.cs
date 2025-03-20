using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LibraryTests.Repositories
{
    public class BookRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public BookRepositoryTests()
        {

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [Fact]
        public async Task GetByISBNAsync_ShouldReturnBook_WhenBookExists()
        {
         
            using (var context = new ApplicationDbContext(_options))
            {
                context.Books.Add(new Book { Id = 1, ISBN = "1234567890", Title = "Book 1", AuthorId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_options))
            {
                var repository = new BookRepository(context);

                var result = await repository.GetByISBNAsync("1234567890");

                Assert.NotNull(result);
                Assert.Equal("Book 1", result.Title);
            }
        }

        [Fact]
        public async Task GetByISBNAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {

            using (var context = new ApplicationDbContext(_options))
            {
                var repository = new BookRepository(context);

                var result = await repository.GetByISBNAsync("9999999999");

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetBooksByAuthorIdAsync_ShouldReturnBooks_WhenAuthorHasBooks()
        {

            using (var context = new ApplicationDbContext(_options))
            {

                context.Books.RemoveRange(context.Books);
                await context.SaveChangesAsync();

                context.Books.AddRange(
                    new Book { Id = 1, ISBN = "1234567890", Title = "Book 1", AuthorId = 1 },
                    new Book { Id = 2, ISBN = "0987654321", Title = "Book 2", AuthorId = 1 },
                    new Book { Id = 3, ISBN = "1122334455", Title = "Book 3", AuthorId = 2 }
                );
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_options))
            {
                var repository = new BookRepository(context);


                var result = await repository.GetBooksByAuthorIdAsync(1);


                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
            }
        }

        [Fact]
        public async Task GetBooksByAuthorIdAsync_ShouldReturnEmptyList_WhenAuthorHasNoBooks()
        {

            using (var context = new ApplicationDbContext(_options))
            {

                context.Books.RemoveRange(context.Books);
                await context.SaveChangesAsync();

                var repository = new BookRepository(context);

  
                var result = await repository.GetBooksByAuthorIdAsync(999);


                Assert.NotNull(result);
                Assert.Empty(result);
            }
        }
    }
}