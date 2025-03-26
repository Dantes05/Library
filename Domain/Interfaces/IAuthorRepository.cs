using Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task<IEnumerable<Author>> GetAuthorsByCountryAsync(string country,
            CancellationToken cancellationToken = default);
        Task<Author?> GetByNameAsync(string name,
            CancellationToken cancellationToken = default);
    }
}