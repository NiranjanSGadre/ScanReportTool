using System.Data;
namespace ScanReportTool.Models
{
    public class ReportResultViewModel
    {
        public DataTable SummaryTable { get; set; }
        public List<DataTable> WeeklyTables { get; set; } = new List<DataTable>();
    }
}
