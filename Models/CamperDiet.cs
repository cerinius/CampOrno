using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampOrno.Models
{
    public class CamperDiet
    {
        public int CamperID { get; set; }
        public Camper Camper { get; set; }

        public int DietaryRestrictionID { get; set; }
        public DietaryRestriction DietaryRestriction { get; set; }

    }
}
