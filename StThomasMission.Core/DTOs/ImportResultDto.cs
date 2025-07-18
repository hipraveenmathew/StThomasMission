using System.Collections.Generic;

namespace StThomasMission.Core.DTOs
{
    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessfullyImported { get; set; }
        public List<FailedRowDto> FailedRows { get; } = new List<FailedRowDto>();
        public bool Success => FailedRows.Count == 0;

        public void AddFailedRow(int rowNumber, string error)
        {
            FailedRows.Add(new FailedRowDto { RowNumber = rowNumber, ErrorMessage = error });
        }
    }
}