using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    public class DashboardSummaryDto
    {
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int GraduatedStudents { get; set; }
        public int MigratedStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int DeletedStudents { get; set; }

        public int TotalFamilies { get; set; }
        public int RegisteredFamilies { get; set; }
        public int UnregisteredFamilies { get; set; }

        public int TotalWards { get; set; }
        public int TotalGroups { get; set; }
        public int TotalActivities { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public double GraduationRate => TotalStudents > 0 ? Math.Round((double)GraduatedStudents / TotalStudents * 100, 2) : 0;
        public double RegistrationRate => TotalFamilies > 0 ? Math.Round((double)RegisteredFamilies / TotalFamilies * 100, 2) : 0;
    }
}