using System;

namespace StThomasMission.Core.DTOs
{
    public class DashboardSummaryDto
    {
        // Student Counts
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int GraduatedStudents { get; set; }
        public int MigratedStudents { get; set; }

        // Family Counts
        public int TotalFamilies { get; set; }
        public int RegisteredFamilies { get; set; }
        public int UnregisteredFamilies { get; set; }

        // Other Counts
        public int TotalWards { get; set; }
        public int TotalGroups { get; set; }

        public DateTime GeneratedAt { get; set; }

        // Calculated Properties
        public double GraduationRate => TotalStudents > 0 ? Math.Round((double)GraduatedStudents / TotalStudents * 100, 2) : 0;
        public double RegistrationRate => TotalFamilies > 0 ? Math.Round((double)RegisteredFamilies / TotalFamilies * 100, 2) : 0;
    }
}