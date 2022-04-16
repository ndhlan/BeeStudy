using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Udemy Id")]
        public int UdemyId { get; set; }

        public string Title { get; set; }

        public string Headline { get; set; }

        [Display(Name = "Course's link")]
        [Required(ErrorMessage = "Please enter URL of your favourite course")]
        [Url]
        [RegularExpression("^https://www.udemy.com/course/.+/$", ErrorMessage = "Please enter course's link from Udemy")]
        public string Url { get; set; }

        public string ImageUrl { get; set; }

        [Display(Name = "List Price")]
        public decimal ListPrice { get; set; }

        [Display(Name = "Last Updated Price")]
        public decimal LastUpdatedPrice { get; set; }

        [Display(Name = "Last Updated Price Date")]
        public DateTime LastUpdatedPriceDate { get; set; }

        [Display(Name = "Current Price")]
        public decimal CurrentPrice { get; set; }

        [Display(Name = "Current Price Date")]
        public DateTime CurrentPriceDate { get; set; }

        [Display(Name = "Discount Expiration")]
        public string DiscountExpiration { get; set; }

        public string Browser { get; set; }

        //Relationships
        public List<Registration> Registrations { get; set; } = new List<Registration>();




    }
}
