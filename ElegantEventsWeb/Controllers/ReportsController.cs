
using ElegantEventsWeb.Data;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace ElegantEventsWeb.Controllers
{
    [Authorize(Roles = "Administrator,Coordinator")]
    public class ReportsController : Controller
    {
        private readonly DatabaseConnection _database;

        private static readonly Dictionary<string, string>
            ReportViews = new()
            {
                ["clients"] = "dbo.View_Client_Events",
                ["staff"] = "dbo.View_Event_Staff",
                ["vendors"] = "dbo.View_Event_Vendors",
                ["venues"] = "dbo.View_Event_Venues",
                ["weddings"] = "dbo.View_Wedding_Details"
            };

        public ReportsController(DatabaseConnection database)
        {
            _database = database;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string report = "clients")
        {
            if (!ReportViews.TryGetValue(report, out string? viewName))
                return BadRequest("Invalid report selected.");

            var table = new DataTable();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM {viewName}";

            using var reader = await command.ExecuteReaderAsync();
            table.Load(reader);

            ViewBag.SelectedReport = report;
            return View(table);
        }
    }
}
