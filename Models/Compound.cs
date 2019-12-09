using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CampOrno.Models
{
    public class Compound
    {
        public Compound()
        {
            this.Campers = new HashSet<Camper>();
            this.CounselorCompounds = new HashSet<CounselorCompound>();
        }
        public int ID { get; set; }

        [Display(Name = "Compound Name")]
        [Required(ErrorMessage = "You cannot leave the compound name blank.")]
        [StringLength(20, ErrorMessage = "Compound name cannot be more than 50 characters long.")]
        public string Name { get; set; }

        [Display(Name = "Counselors")]
        public ICollection<CounselorCompound> CounselorCompounds { get; set; }

        [Display(Name = "Campers")]
        public ICollection<Camper> Campers { get; set; }
    }
}
