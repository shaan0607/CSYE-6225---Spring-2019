using System.ComponentModel.DataAnnotations;

namespace trial3.Controller.Model
{
    public class User 
    {
        public User(string email, string password){

            this.email = email;
            this.password = password;
        }
        [Required]
        [EmailAddress]
        [Key]
        public string email{
            get;
            set;
        }
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[#$^+=!*()@%&]).{8,}$")]
        public string password{
            get;
            set; 
        }
    }
}