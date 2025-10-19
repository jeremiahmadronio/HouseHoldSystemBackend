using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models
{
    public class PriceReport
    {
        [Key]
        public int ReportId { get; set; }
        public string FileName { get; set; }
        public string ReportWeek { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public string UploadedBy { get; set; } = "Admin";
        public ICollection<ProductPrice> Prices { get; set; }
    }
}