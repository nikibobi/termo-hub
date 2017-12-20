using System.ComponentModel.DataAnnotations;

namespace TermoHub.ViewModels
{
    public class Login
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(50)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}