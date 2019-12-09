using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CampOrno.Models
{
    public class Counselor : Auditable
    {
        public Counselor()
        {
            this.Campers = new HashSet<Camper>();
            this.CounselorCompounds = new HashSet<CounselorCompound>();
        }
        public int ID { get; set; }

        [Display(Name = "Counselor")]
        [DisplayFormat(NullDisplayText ="None Assigned")]
        public string FullName
        {
            get
            {
                if(String.IsNullOrEmpty(Nickname))
                {
                    return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
                }else
                {
                    return Nickname;
                }
            }
        }

        [Display(Name = "Counselor")]
        public string FormalName
        {
            get
            {
                return (string.IsNullOrEmpty(Nickname) ? "" :
                        Nickname + " - ") 
                        + LastName + ", " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + (char?)MiddleName[0] + ".").ToUpper());
            }
        }

        [Display(Name = "SIN")]
        public string SINDisplay
        {
            get
            {
                return SIN.Substring(0, 3) + "-" + SIN.Substring(3, 3) + "-" + SIN.Substring(6, 3);
            }
        }


        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(50, ErrorMessage = "Middle name cannot be more than 50 characters long.")]
        public string MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(100, ErrorMessage = "Last name cannot be more than 100 characters long.")]
        public string LastName { get; set; }

        
        [StringLength(50, ErrorMessage = "If entered, the nickame cannot be more than 50 characters long.")]
        public string Nickname { get; set; }


        [Required(ErrorMessage = "You cannot leave the SIN blank.")]
        [RegularExpression("^\\d{9}$", ErrorMessage = "The SIN must be exactly 9 numeric digits.")]
        [StringLength(9)]
        public string SIN { get; set; }

        [Display(Name = "Campers Mentored")]
        public ICollection<Camper> Campers { get; set; }

        [Display(Name = "Compound Assignments")]
        public ICollection<CounselorCompound> CounselorCompounds { get; set; }

    }
}
