/*
 * Author      : Ali Alsaffar
 * Student ID  : 202301152
 * Stage       : Stage 3 - Authentication & Identity
 * Description : View model carrying the email, password, and remember-me fields submitted by the login form.
 */
using System.ComponentModel.DataAnnotations;

namespace ClinicMVC.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
