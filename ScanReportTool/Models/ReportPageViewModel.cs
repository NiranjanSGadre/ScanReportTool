namespace ScanReportTool.Models
{
    public class ReportPageViewModel
    {
        public ReportFilterViewModel Filter { get; set; } = new ReportFilterViewModel();
        public ReportResultViewModel? ReportData { get; set; }
    }
}
