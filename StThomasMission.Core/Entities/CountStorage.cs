using System.ComponentModel.DataAnnotations;

namespace StThomasMission.Core.Entities
{
    /// <summary>
    /// Stores sequence counters to avoid expensive MAX() lookups on large tables.
    /// Used for generating new ChurchRegistrationNumber and TemporaryID.
    /// </summary>
    public class CountStorage
    {
        /// <summary>
        /// The unique name of the counter, which serves as the primary key.
        /// e.g., "ChurchRegistrationNumber", "TemporaryID"
        /// </summary>
        [Key]
        [StringLength(100)]
        public string CounterName { get; set; } = string.Empty;

        /// <summary>
        /// The last integer value used for this counter.
        /// </summary>
        [Required]
        public int LastValue { get; set; }

        /// <summary>
        /// A timestamp for optimistic concurrency control to prevent race conditions.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}