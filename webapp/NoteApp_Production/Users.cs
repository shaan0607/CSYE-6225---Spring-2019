using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace trial3
{
    public partial class Users
    {
        //public Users(string )

        [Required]
        [Key]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$")]
        public string Password { get; set; }
    }
}
