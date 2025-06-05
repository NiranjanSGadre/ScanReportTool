using Microsoft.AspNetCore.Mvc;
using ScanReportTool.Models;
using System.Globalization;
using Microsoft.Data.SqlClient;
using DataAccessLayer;
using System.Data;

namespace ScanReportTool.Controllers
{
    public class ReportController : Controller
    {
        private readonly string _connectionString;
        public ReportController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IActionResult> Index()
        {
            var model = new ReportPageViewModel
            {
                Filter = new ReportFilterViewModel
                {
                    BankList = await GetBanks(),
                    YearList = GetYearList(),
                    SelectedMonth = DateTime.Now.Month,
                    SelectedYear = DateTime.Now.Year
                }
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ReportFilterViewModel filter)
        {
            var reportResult = new ReportResultViewModel();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("GetReport", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BankName", filter.SelectedBank);
                cmd.Parameters.AddWithValue("@Month", filter.SelectedMonth);
                cmd.Parameters.AddWithValue("@Year", filter.SelectedYear);

                DataSet ds = new DataSet();
                adapter.Fill(ds);

                if (ds.Tables.Count > 0)
                {
                    reportResult.SummaryTable = ds.Tables[ds.Tables.Count-1];
                    for (int i = 0; i < ds.Tables.Count-1; i++)
                    {
                        reportResult.WeeklyTables.Add(ds.Tables[i]);
                    }
                }
            }

            var model = new ReportPageViewModel
            {
                Filter = filter,
                ReportData = reportResult
            };

            // Rebind dropdown lists if needed
            model.Filter.BankList = await GetBanks();
            model.Filter.YearList = GetYearList();

            return View(model);
        }

        private async Task<List<string>> GetBanks()
        {
            DatabaseHelper databaseHelper = new DatabaseHelper(_connectionString);
            DataTable dt=  await databaseHelper.GetDataTable("GetBanks");
            List<string> uniqueBankNames = dt.AsEnumerable()
                    .Select(row => row.Field<string>("BankName"))
                    .Distinct()
                    .ToList();
            return uniqueBankNames;
        }

        private List<int> GetYearList()
        {
            var currentYear = DateTime.Now.Year;
            return Enumerable.Range(currentYear - 5, 6).ToList(); // e.g. 2019–2024
        }
    }
}
