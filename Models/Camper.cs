using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CampOrno.Models
{
     
    public class Camper : Auditable, IValidatableObject
    {
        public Camper()
        {
            CamperDiets = new HashSet<CamperDiet>();

        }
        public int ID { get; set; }

        [Display(Name = "Camper")]
        public string FullName
        {
            get
            {
                return FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? " " :
                        (" " + (char?)MiddleName[0] + ". ").ToUpper())
                    + LastName;
            }
        }

        public string FormalName
        {
            get
            {
                return LastName + ", " + FirstName
                    + (string.IsNullOrEmpty(MiddleName) ? "" :
                        (" " + (char?)MiddleName[0] + ".").ToUpper());
            }
        }
        public string Age
        {
            get
            {
                DateTime today = DateTime.Today;
                int a = today.Year - DOB.Year
                    - ((today.Month < DOB.Month || (today.Month == DOB.Month && today.Day < DOB.Day) ? 1 : 0));
                return a.ToString(); /*Note: You could add .PadLeft(3) but spaces disappear in a web page. */
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

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "You must enter the date of birth.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "You cannot leave the GENDER blank.")]
        [RegularExpression("^\\{|M|F|N|T|O|}$", ErrorMessage = "Gender must be one of M, F, N, T OR O")]
        [StringLength(1)]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string eMail { get; set; }

        [Display(Name = "Emergency Phone")]
        [Required(ErrorMessage = "Emergency Phone number is required.")]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [DisplayFormat(DataFormatString = "{0:(###) ###-####}", ApplyFormatInEditMode = false)]
        public Int64 Phone { get; set; }

        [Display(Name = "Lead Counselor")]
        public int? CounselorID { get; set; }
        [Display(Name = "Lead Counselor")]
        public Counselor Counselor { get; set; }

        [Display(Name = "Compound")]
        public int CompoundID { get; set; }
        public Compound Compound { get; set; }

        [Display(Name = "Dietary Restrictions")]

        [ScaffoldColumn(false)]
        public byte[] imageContent { get; set; }

        [StringLength(256)]
        [ScaffoldColumn(false)]
        public string imageMimeType { get; set; }

        [StringLength(100, ErrorMessage = "The name of the file cannot be more than 100 characters.")]
        [Display(Name = "File Name")]
        [ScaffoldColumn(false)]
        public string imageFileName { get; set; }


        public ICollection<CamperDiet> CamperDiets { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //This is an alternate way to enforce the gender value restriction without a RegEx
            string gender = "MFNTO";
            if (!gender.Contains(Gender))
            {
                yield return new ValidationResult("Gender Code must be one of M, F, N, T OR O.", new[] { "Gender" });
            }
            if (int.Parse(Age)<4)
            {
                yield return new ValidationResult("Camper must be at least 4 years old.", new[] { "DOB" });
            }
        }
    }
}
