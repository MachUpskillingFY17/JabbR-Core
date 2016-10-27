using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JabbR_Core.ViewModels
{
    public class ChangeUsernameViewModel
    {
        [Required(ErrorMessage = "The Username field is required")]
        public string username { get; set; }

        [Required(ErrorMessage = "The Confirm Username field is required")]
        [Compare("username", ErrorMessage = "The username and confirmation username do not match.")]
        public string confirmUsername { get; set; }

        [Required(ErrorMessage = "The Password field is required")]
        [DataType(DataType.Password)]
        public string password { get; set; } // need to compare with real password, but that is not in the database right now!
    }
}
