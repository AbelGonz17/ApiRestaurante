using System.ComponentModel.DataAnnotations;

namespace RestauranteAPI.DTOs
{
    public class CredencialesUsuarioDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        public string name { get; set; }
    }
}
