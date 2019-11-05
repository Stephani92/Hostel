using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class ForgotPassword
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}