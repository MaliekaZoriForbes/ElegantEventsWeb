
using Microsoft.AspNetCore.Mvc;
using ElegantEventsWeb.Data;
using ElegantEventsWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace ElegantEventsWeb.Controllers
{
    [Authorize(Roles = "Administrator,Coordinator")]
    public class VenuesController : Controller
    {
        private readonly DatabaseConnection _database;

        public VenuesController(DatabaseConnection database)
        {
            _database = database;
        }

        public async Task<IActionResult> Index()
        {
            var venues = new List<Venue>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT venueName, rentalFee, capacity, location
                FROM dbo.venue
                ORDER BY venueName;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                venues.Add(new Venue
                {
                    VenueName = reader.GetString(0),
                    RentalFee = reader.GetDecimal(1),
                    Capacity = reader.IsDBNull(2)
                        ? "" : reader.GetString(2),
                    Location = reader.IsDBNull(3)
                        ? "" : reader.GetString(3)
                });
            }

            return View(venues);
        }
    

[HttpGet]
        public async Task<IActionResult> CheckAvailability(
    string? venueName, DateTime? eventDate)
        {
            ViewBag.VenueName = venueName;
            ViewBag.EventDate = eventDate;

            var venues = new List<string>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT venueName FROM dbo.venue ORDER BY venueName";

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    venues.Add(reader.GetString(0));
                }
            }

            ViewBag.Venues = venues;

            if (!string.IsNullOrWhiteSpace(venueName)
                && eventDate.HasValue)
            {
                using var command = connection.CreateCommand();

                command.CommandText = @"
            SELECT COUNT(*)
            FROM dbo.held h
            INNER JOIN dbo.eventt e
                ON h.event_ID = e.event_ID
            WHERE h.venueName = @VenueName
              AND e.event_Date = @EventDate;
        ";

                command.Parameters.AddWithValue(
                    "@VenueName", venueName);

                command.Parameters.AddWithValue(
                    "@EventDate", eventDate.Value.Date);

                int bookings = Convert.ToInt32(
                    await command.ExecuteScalarAsync());

                ViewBag.IsAvailable = bookings == 0;
            }

            return View();
        }

    }
}
