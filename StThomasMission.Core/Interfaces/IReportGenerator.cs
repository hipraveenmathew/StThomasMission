using StThomasMission.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StThomasMission.Services.Interfaces
{
    public interface IReportGenerator<T> where T : class
    {
        Task<byte[]> GenerateReportAsync(ReportFormat format, IEnumerable<T> data, string reportTitle);
    }
}