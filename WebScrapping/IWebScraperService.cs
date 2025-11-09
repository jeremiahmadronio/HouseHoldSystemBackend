

namespace WebApplication2.WebScrapping
{

    public interface IWebScraperService
    {
        Task CheckAndDownloadNewPDFsAsync();
    }
}
