using System.ComponentModel.DataAnnotations;

namespace RestauranteAPI.DTOs
{
    public class CrendecialesLoginDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
