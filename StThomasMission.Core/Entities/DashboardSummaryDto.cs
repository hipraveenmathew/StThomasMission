using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StThomasMission.Core.Entities
{
    public class DashboardSummaryDto
    {
        // Students
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int GraduatedStudents { get; set; }
        public int MigratedStudents { get; set; }

        // Families
        public int TotalFamilies { get; set; }
        public int RegisteredFamilies { get; set; }
        public int UnregisteredFamilies { get; set; }

        // Groups and Activities
        public int TotalGroups { get; set; }
        public int TotalActivities { get; set; }
    }
}
