using AutoMapper;
using WebApplication2.dto.PriceReportDTO;
using WebApplication2.repositories;
using WebApplication2.models;


namespace WebApplication2.service
{
	public class PriceReportService { 
	
		private readonly IPriceReportRepository _priceReportRepository;

		public PriceReportService(IPriceReportRepository priceReportRepository) {
			_priceReportRepository = priceReportRepository;
		}


		public async Task<IEnumerable<DisplayReportDTO>> getAllAsync() { 
		
			var report = await _priceReportRepository.getAllAsync();

			var result = report.Select(x => new DisplayReportDTO
			{

				ReportId = x.ReportId,
				FileName = x.FileName,
				ReportWeek = x.ReportWeek,
				UploadedBy = x.UploadedBy,
				UploadDate = x.UploadDate,
			});

			return result;
		
		}




	}
}