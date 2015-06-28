using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RSS.Models
{
    public class RSSUser
    {
        [Key]
        public int Id { get; set; }

        public List<channel> subscriptions { get; set; }
        public List<Categories> categories { get; set; }
        public channel currentChannel { get; set; }
        public string currentArticle { get; set; }
        public Categories currentCategory { get; set; }

        public bool isAdmin { get; set; }

        [Required(ErrorMessage = "Please enter your first name.")]
        [Display(Name = "First Name:")]
        [MaxLength(50, ErrorMessage = "Name is too long (50 chars max).")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name.")]
        [Display(Name = "Last Name:")]
        public string LastName { get; set; }

        //we can even specify a data type using attributes!
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Please enter your email address.")]
        [Display(Name = "Email Address:")]
        public string Email { get; set; }

        public int order { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Please enter your password.")]
        [Display(Name = "Password:")]
        public string Password { get; set; }

    }
}