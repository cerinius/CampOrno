using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampOrno.Models
{
    public class CounselorCompound
    {
        public int CounselorID { get; set; }
        public Counselor Counselor { get; set; }
        public int CompoundID { get; set; }
        public Compound Compound { get; set; }

    }
}
