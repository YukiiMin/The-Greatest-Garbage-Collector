namespace GarbageCollection.Common.Models
{
    public class WorkArea
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;

        /// <summary>"District" hoặc "Ward"</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>null nếu là District; trỏ đến District nếu là Ward</summary>
        public Guid? ParentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public WorkArea? Parent { get; set; }
        public List<WorkArea> Children { get; set; } = new();
    }
}
