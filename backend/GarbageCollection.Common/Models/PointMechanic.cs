namespace GarbageCollection.Common.Models
{
    /// <summary>Tiêu chí tính điểm cho một loại rác cụ thể.</summary>
    public class WasteTypeCriteria
    {
        /// <summary>Số điểm thưởng.</summary>
        public decimal Points { get; set; }

        /// <summary>Khối lượng tối thiểu để được tính điểm (gram).</summary>
        public decimal MinWeightGrams { get; set; }
    }

    /// <summary>
    /// Cơ chế tính điểm cho một danh mục — lưu dạng JSONB.
    /// </summary>
    public class PointMechanic
    {
        public WasteTypeCriteria Organic { get; set; } = new();
        public WasteTypeCriteria Recyclable { get; set; } = new();
        public WasteTypeCriteria NonRecyclable { get; set; } = new();
    }
}
