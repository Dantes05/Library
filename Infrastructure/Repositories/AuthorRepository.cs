using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class AuthorRepository : Repository<Author>, IAuthorRepository
    {
        public AuthorRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Author>> GetAuthorsByCountryAsync(string country)
        {
            return await _context.Authors.Where(a => a.Country == country).ToListAsync();
        }
    }
}
