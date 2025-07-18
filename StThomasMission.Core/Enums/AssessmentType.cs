using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Enums
{
    /// <summary>
    /// Specifies the type of a student assessment.
    /// </summary>
    public enum AssessmentType
    {
        [Display(Name = "Class Assessment")]
        ClassAssessment,

        [Display(Name = "Semester Exam")]
        SemesterExam
    }
}