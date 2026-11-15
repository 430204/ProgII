using System.ComponentModel.DataAnnotations;

namespace InmoWeb.DTOs
{
    public class UserLoginDTO
    {
        [Required]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        public string DNI { get; set; } = string.Empty;
    }
}
