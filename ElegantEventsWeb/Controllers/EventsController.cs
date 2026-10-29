
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ElegantEventsWeb.Data;
using ElegantEventsWeb.Models;

namespace ElegantEventsWeb.Controllers
{
    public class EventsController : Controller
    {
        private readonly DatabaseConnection _database;

        public EventsController(DatabaseConnection database)
        {
            _database = database;
        }

        public async Task<IActionResult> Index()
        {
            var events = new List<Event>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT e.event_ID, e.event_name, e.event_Date,
                       e.numberOfGuests, e.budget, e.client_ID,
                       ISNULL(h.venueName, '') AS venueName
                FROM dbo.eventt e
                LEFT JOIN dbo.held h
                    ON e.event_ID = h.event_ID
                ORDER BY e.event_Date;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                events.Add(new Event
                {
                    Event_ID = reader.GetString(0).Trim(),
                    Event_Name = reader.GetString(1),
                    Event_Date = reader.GetDateTime(2),
                    NumberOfGuests = reader.GetInt32(3),
                    Budget = reader.GetDecimal(4),
                    Client_ID = reader.GetInt32(5),
                    VenueName = reader.GetString(6)
                });
            }

            return View(events);
        }
    }
}
