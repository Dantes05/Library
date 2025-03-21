using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class BookUpdateDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "ISBN обязателен.")]
        [StringLength(20, ErrorMessage = "ISBN не должен превышать 20 символов.")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Название обязательно.")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Жанр не должен превышать 100 символов.")]
        public string Genre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Автор обязателен.")]
        public int AuthorId { get; set; }

        public IFormFile? Image { get; set; }
    }
}
