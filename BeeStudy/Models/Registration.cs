using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeeStudy.Models
{
    public class Registration
    {
        public int CourseId { get; set; }
        public int LearnerId { get; set; }

        public Course Course { get; set; }
        public Learner Learner { get; set; }


    }
}
