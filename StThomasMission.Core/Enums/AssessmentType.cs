using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    public enum AssessmentType
    {
        [Display(Name = "Class Assessment")]
        ClassAssessment,
        [Display(Name = "Semester Exam")]
        SemesterExam
    }
}
