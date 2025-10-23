namespace WebApplication2.dto.PriceReportDTO
{
    public class DisplayReportDTO
    {

        public int ReportId { get; set; }
        public string FileName { get; set; }
        public string ReportWeek { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? UploadDate { get; set; }
    }
}

