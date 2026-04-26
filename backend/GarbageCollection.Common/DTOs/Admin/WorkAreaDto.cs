using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GarbageCollection.Common.DTOs.Admin
{
    public class WorkAreaDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("parent_id")]
        public Guid? ParentId { get; set; }

        [JsonPropertyName("parent_name")]
        public string? ParentName { get; set; }

        [JsonPropertyName("children")]
        public List<WorkAreaDto> Children { get; set; } = new();

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class SaveWorkAreaData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>"District" hoặc "Ward"</summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Bắt buộc nếu Type = "Ward"</summary>
        [JsonPropertyName("parent_id")]
        public Guid? ParentId { get; set; }
    }

    public class SaveWorkAreaRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public SaveWorkAreaData Data { get; set; } = null!;
    }

    // ── Tạo District (không cần parent_id) ───────────────────────────────────

    public class CreateDistrictData
    {
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class CreateDistrictRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public CreateDistrictData Data { get; set; } = null!;
    }

    // ── Tạo Ward (parent_id bắt buộc) ────────────────────────────────────────

    public class CreateWardData
    {
        [Required]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("parent_id")]
        public Guid? ParentId { get; set; }
    }

    public class CreateWardRequest
    {
        [Required]
        [JsonPropertyName("data")]
        public CreateWardData Data { get; set; } = null!;
    }
}
