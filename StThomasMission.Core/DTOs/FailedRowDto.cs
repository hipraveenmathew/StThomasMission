namespace StThomasMission.Core.DTOs
{
    public class FailedRowDto
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}