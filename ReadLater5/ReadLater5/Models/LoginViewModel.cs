using System.ComponentModel.DataAnnotations;

namespace ReadLater5.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
