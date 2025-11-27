    namespace WebApplication2.dto.PriceAnalyticsDTO
    {
        public class PriceFilterRequest
        {
            public string TimeRange { get; set; } = "Daily"; // Daily, Weekly, Monthly
            public string Category { get; set; } = "All";
        }
    }
