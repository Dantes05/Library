using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryTests.Persistence
{
    public class TestDbContext : ApplicationDbContext
    {
        public TestDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, ISBN = "1234567890", Title = "Book 1", AuthorId = 1 },
                new Book { Id = 2, ISBN = "0987654321", Title = "Book 2", AuthorId = 1 },
                new Book { Id = 3, ISBN = "1122334455", Title = "Book 3", AuthorId = 2 }
            );
        }
    }
}
