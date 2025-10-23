using WebApplication2.data;
using WebApplication2.models;
using Microsoft.EntityFrameworkCore;
namespace WebApplication2.repositories.repository
{ 

    public class PriceReportRepository : IPriceReportRepository
    {
        private readonly ApplicationDbContext _context;
        public PriceReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        //ADD 
        public async Task<PriceReport> AddAsync(PriceReport report)
        {
            _context.PriceReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        //SAVE
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PriceReport>> getAllAsync() => await _context.PriceReports.ToListAsync();
    }

}