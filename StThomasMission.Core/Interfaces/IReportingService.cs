namespace StThomasMission.Core.Interfaces
{
    public interface IReportingService
    {
        Task<byte[]> GenerateStudentReportPdfAsync(int studentId);
        Task<byte[]> GenerateClassReportPdfAsync(string grade);
        Task<byte[]> GenerateOverallCatechismReportPdfAsync();
        Task<byte[]> GenerateFamilyReportPdfAsync();
        Task<byte[]> GenerateStudentReportExcelAsync(int studentId);
        Task<byte[]> GenerateClassReportExcelAsync(string grade);
        Task<byte[]> GenerateOverallCatechismReportExcelAsync();
        Task<byte[]> GenerateFamilyReportExcelAsync();
    }
}