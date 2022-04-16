using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Models
{
    public class Learner
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Is Registered User")]
        public bool IsRegisteredUser { get; set; }

        //Relationships
        public List<Registration> Registrations { get; set; } = new List<Registration>();

    }
}
