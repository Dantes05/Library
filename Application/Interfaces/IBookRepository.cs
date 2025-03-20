using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<Book?> GetByISBNAsync(string isbn);
        Task<IEnumerable<Book>> GetBooksByAuthorIdAsync(int authorId);
        IQueryable<Book> GetAllQueryable();
    }
}
