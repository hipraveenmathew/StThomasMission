using StThomasMission.Core.Enums;
using System;
using System.Collections.Generic;

namespace StThomasMission.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public int FamilyMemberId { get; set; }
        public string Grade { get; set; }
        public int AcademicYear { get; set; }
        public int GroupId { get; set; }
        public string StudentOrganisation { get; set; }
        public StudentStatus Status { get; set; }
        public string? MigratedTo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public FamilyMember FamilyMember { get; set; }
        public Group Group { get; set; }
        public List<Attendance> Attendances { get; set; }
        public List<Assessment> Assessments { get; set; }
        public List<StudentGroupActivity> StudentGroupActivities { get; set; }
        public List<StudentAcademicRecord> AcademicRecords { get; set; } // Added navigation property
    }
}