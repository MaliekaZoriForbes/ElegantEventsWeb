
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElegantEventsWeb.Models;
using ElegantEventsWeb.Data;

namespace ElegantEventsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DatabaseConnection _database;

        public HomeController(
            ILogger<HomeController> logger,
            DatabaseConnection database)
        {
            _logger = logger;
            _database = database;
        }


        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = @"
        SELECT
            (SELECT COUNT(*) FROM dbo.client),
            (SELECT COUNT(*) FROM dbo.eventt),
            (SELECT COUNT(*) FROM dbo.venue),
            (SELECT COUNT(*) FROM dbo.payment);
    ";

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                model.TotalClients = reader.GetInt32(0);
                model.TotalEvents = reader.GetInt32(1);
                model.TotalVenues = reader.GetInt32(2);
                model.TotalPayments = reader.GetInt32(3);
            }

            return View(model);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                using var connection = _database.GetConnection();

                await connection.OpenAsync();

                using var command = connection.CreateCommand();

                command.CommandText = "SELECT DB_NAME()";

                var result = await command.ExecuteScalarAsync();

                return Content(
                    "SUCCESS! Connected to database: " + result
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection failed.");

                return Content(
                    "Database connection failed. Check the Visual Studio Output window for details."
                );
            }
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}
