namespace StThomasMission.Core.Interfaces
{
    public interface IImportService
    {
        Task<bool> ImportFamiliesAndStudentsAsync(Stream fileStream);
    }
}