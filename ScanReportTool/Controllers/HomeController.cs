using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ScanReportTool.Models;
using System.Data;
using System.Diagnostics;
using System.Data.OleDb;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace ScanReportTool.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<HomeController> _logger;
        //private readonly string _connectionString = "Your SQL Server Connection String";
        //private readonly string _connectionString = "Server=DESKTOP-C9MBSDQ\\SQLEXPRESS;Database=BankScan;User Id=sa;Password=company69;TrustServerCertificate=True;";
        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ExcelUpload()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [Route("UploadFiles")]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files were selected.");

            foreach (var file in files)
            {
                DataTable dataTable = new DataTable();
                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                string tempFilePath = Path.Combine(Path.GetTempPath(), file.FileName);
                // Save the file temporarily
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    // Read Excel data into a DataTable

                    dataTable = ReadExcelFile(file);
                    // Delete the temporary file after reading (optional)
                    System.IO.File.Delete(tempFilePath);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        await UploadExcelDataToDatabaseAsync(dataTable);
                    }

                }
                else if (fileExtension == ".csv")
                {
                    dataTable = ReadCsvFile(file);
                    // Delete the temporary file after reading (optional)
                    System.IO.File.Delete(tempFilePath);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        await UploadCsvDataToDatabaseAsync(dataTable);
                    }

                }
                else
                {
                    ViewBag.Message = "Invalid file format. Only Excel (.xls, .xlsx) and CSV (.csv) are supported.";
                    return View("Index");
                }

                
                //if (Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                //{
                //    // Upload DataTable to SQL Server
                //}
            }

            //return Ok("All Excel files have been processed and uploaded.");
            ViewBag.Message = "All files have been uploaded successfully.";
            return View("ExcelUpload"); // Stay on the same page
        }
        // Method to read Excel file and return DataTable
        // Method to read Excel file and return DataTable
        private DataTable ReadExcelFile(IFormFile file)
        {
            DataTable dt = new DataTable();
            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheets.First();
                bool firstRow = true;

                foreach (var row in worksheet.RowsUsed())
                {
                    if (firstRow)
                    {
                        foreach (var cell in row.CellsUsed())
                            dt.Columns.Add(cell.Value.ToString());
                        firstRow = false;
                    }
                    else
                    {
                        try
                        {
                            dt.Rows.Add(row.CellsUsed().Select(c => c.Value.ToString()).ToArray());
                        }
                        catch (Exception ex)
                        {


                        }

                    }
                }
            }
            return dt;
        }
        private DataTable ReadCsvFile(IFormFile file)
        {
            DataTable dt = new DataTable();
           

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,

            };

            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
            using var csv = new CsvReader(reader, config);

            using var dr = new CsvDataReader(csv);

            dt.Load(dr);
            //using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
            //{
            //    bool isFirstRow = true;
            //    while (!reader.EndOfStream)
            //    {
            //        string line = reader.ReadLine().Replace("\"", "");
            //        string[] values = line.Split(',',StringSplitOptions.RemoveEmptyEntries);

            //        if (isFirstRow)
            //        {
            //            foreach (var column in values)
            //            {
            //                dt.Columns.Add(column.Trim().Replace("\"","")); // Use first row as headers
            //            }
            //            isFirstRow = false;
            //        }
            //        else
            //        {
            //            try
            //            {
            //                dt.Rows.Add(values);
            //            }
            //            catch (Exception ex)
            //            {

                            
            //            }
                        
            //        }
            //    }
            //}
            return dt;
        }
        // Method to upload DataTable to SQL Server
        private async Task UploadExcelDataToDatabaseAsync(DataTable dataTable)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "ScanData";
                    try
                    {
                        await bulkCopy.WriteToServerAsync(dataTable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading data: {ex.Message}");
                    }
                }
            }
        }

        private async Task UploadCsvDataToDatabaseAsync(DataTable dataTable)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "ScanData";
                    try
                    {
                        await bulkCopy.WriteToServerAsync(dataTable);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error uploading data: {ex.Message}");
                    }
                }
            }
        }
    }
}
