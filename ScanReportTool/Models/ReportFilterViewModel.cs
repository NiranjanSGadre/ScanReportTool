namespace ScanReportTool.Models
{
    public class ReportFilterViewModel
    {
        public string SelectedBank { get; set; }
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }

        public List<string> BankList { get; set; }
        public List<int> YearList { get; set; }
    }
}
