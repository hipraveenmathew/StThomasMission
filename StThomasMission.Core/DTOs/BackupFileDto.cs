using System;

namespace StThomasMission.Core.DTOs
{
    public class BackupFileDto
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; } // Size in bytes
        public DateTime CreatedDate { get; set; }
    }
}