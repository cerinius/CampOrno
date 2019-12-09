using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CampOrno.Models
{
    public class DietaryRestriction
    {
        public int ID { get; set; }

        [Display(Name = "Dietary Restriction")]
        [Required(ErrorMessage = "You cannot leave the Dietary Restriction blank.")]
        [StringLength(50, ErrorMessage = "Dietary Restriction cannot be more than 50 characters long.")]
        public string Name { get; set; }

        [Display(Name = "Campers")]
        public ICollection<CamperDiet> CamperDiets { get; set; }
    }
}
