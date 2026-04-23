using System.ComponentModel.DataAnnotations;

namespace GarbageCollection.Common.DTOs.Collector
{
    public class StartShiftRequest
    {
        [Required]
        public StartShiftData Data { get; set; } = null!;
    }

    public class StartShiftData
    {
        [Required]
        public int TeamId { get; set; }

        /// <summary>Format: YYYY-MM-DD</summary>
        [Required]
        public DateOnly Date { get; set; }
    }
}
