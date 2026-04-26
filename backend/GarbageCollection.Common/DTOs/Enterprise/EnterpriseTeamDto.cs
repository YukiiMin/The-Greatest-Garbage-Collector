namespace GarbageCollection.Common.DTOs.Enterprise
{
    public class EnterpriseTeamDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool InWork { get; set; }
        public bool IsActive { get; set; }
        public int MemberCount { get; set; }
        public Guid CollectorId { get; set; }
        public decimal TotalCapacity { get; set; }
    }
}
