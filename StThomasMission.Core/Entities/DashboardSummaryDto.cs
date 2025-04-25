namespace StThomasMission.Core.Entities
{
    public class DashboardSummaryDto
    {
        // Students
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int GraduatedStudents { get; set; }
        public int MigratedStudents { get; set; }
        public int InactiveStudents { get; set; }
        public int DeletedStudents { get; set; }

        // Families
        public int TotalFamilies { get; set; }
        public int RegisteredFamilies { get; set; }
        public int UnregisteredFamilies { get; set; }

        // Wards
        public int TotalWards { get; set; } // Added to track ward count

        // Groups & Activities
        public int TotalGroups { get; set; }
        public int TotalActivities { get; set; }

        // Timestamps
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Optional Performance Ratios
        public double GraduationRate => TotalStudents > 0 ? Math.Round((double)GraduatedStudents / TotalStudents * 100, 2) : 0;
        public double RegistrationRate => TotalFamilies > 0 ? Math.Round((double)RegisteredFamilies / TotalFamilies * 100, 2) : 0;
    }
}
