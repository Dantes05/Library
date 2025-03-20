﻿namespace LibraryApp.DTOs
{
    public class BookCreateDto
    {
        public string ISBN { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public int AuthorId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
