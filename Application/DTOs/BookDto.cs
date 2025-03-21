using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public int AuthorId { get; set; }
        public DateTime? TakenAt { get; set; }
        public DateTime? ReturnAt { get; set; }
        public string AuthorName { get; set; }

        public string? ImagePath { get; set; }
    }
}
