using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateAuthorDto
    {
        [Required(ErrorMessage = "Имя обязательно.")]
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна.")]
        [StringLength(100, ErrorMessage = "Фамилия не должна превышать 100 символов.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Дата рождения обязательна.")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Страна обязательна.")]
        [StringLength(100, ErrorMessage = "Страна не должна превышать 100 символов.")]
        public string Country { get; set; } = string.Empty;
    }

}
