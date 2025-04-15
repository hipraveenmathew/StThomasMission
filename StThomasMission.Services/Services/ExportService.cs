using StThomasMission.Core.Entities;
using StThomasMission.Core.Interfaces;
using System.Text;
using System.Threading.Tasks;

namespace StThomasMission.Services
{
    public class ExportService : IExportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> ExportFamiliesToCsvAsync()
        {
            var families = await _unitOfWork.Families.GetAllAsync();
            var sb = new StringBuilder();
            sb.AppendLine("FamilyName,Ward,IsRegistered,ChurchRegistrationNumber,TemporaryID,Status,CreatedDate");

            foreach (var f in families)
            {
                sb.AppendLine($"{f.FamilyName},{f.Ward},{f.IsRegistered},{f.ChurchRegistrationNumber},{f.TemporaryID},{f.Status},{f.CreatedDate:yyyy-MM-dd}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportStudentsToCsvAsync()
        {
            var students = await _unitOfWork.Students.GetAllAsync();
            var sb = new StringBuilder();
            sb.AppendLine("FullName,Grade,AcademicYear,Group,Status");

            foreach (var s in students)
            {
                var familyMember = await _unitOfWork.FamilyMembers.GetByIdAsync(s.FamilyMemberId);
                sb.AppendLine($"{familyMember.FirstName} {familyMember.LastName},{s.Grade},{s.AcademicYear},{s.Group},{s.Status}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportAttendanceToCsvAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var attendance = await _unitOfWork.Attendances.GetAllAsync();
            var records = attendance.Where(a =>
                (!startDate.HasValue || a.Date >= startDate) &&
                (!endDate.HasValue || a.Date <= endDate));

            var sb = new StringBuilder();
            sb.AppendLine("StudentId,Date,Status,Description");

            foreach (var a in records)
            {
                sb.AppendLine($"{a.StudentId},{a.Date:yyyy-MM-dd},{a.Status},{a.Description}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
