using System.ComponentModel.DataAnnotations;

namespace JabbR_Core.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}