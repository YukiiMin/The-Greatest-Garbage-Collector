namespace GarbageCollection.Common.DTOs.Enterprise
{
    public class RejectReportData
    {
        public string? Reason { get; set; }
    }

    public class RejectReportRequest
    {
        public RejectReportData Data { get; set; } = new();
    }
}
