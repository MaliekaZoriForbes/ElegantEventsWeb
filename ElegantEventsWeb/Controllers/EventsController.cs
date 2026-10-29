
using ElegantEventsWeb.Data;
using ElegantEventsWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ElegantEventsWeb.Controllers
{
    [Authorize(Roles = "Administrator,Coordinator")]
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
    

[HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new Event
            {
                Event_Date = DateTime.Today.AddDays(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            if (string.IsNullOrWhiteSpace(model.Event_ID) ||
                string.IsNullOrWhiteSpace(model.Event_Name) ||
                string.IsNullOrWhiteSpace(model.VenueName) ||
                model.Event_Date.Date < DateTime.Today ||
                model.NumberOfGuests <= 0 ||
                model.Budget <= 0)
            {
                ModelState.AddModelError("",
                    "Enter valid event details and a future date.");
                await LoadDropdowns();
                return View(model);
            }

            try
            {
                using var connection = _database.GetConnection();
                await connection.OpenAsync();

                using var command = new SqlCommand(
                    "dbo.tr_book_event_venue", connection);

                command.CommandType =
                    System.Data.CommandType.StoredProcedure;

                command.Parameters.AddWithValue(
                    "@event_ID", model.Event_ID);
                command.Parameters.AddWithValue(
                    "@event_name", model.Event_Name);
                command.Parameters.AddWithValue(
                    "@event_Date", model.Event_Date.Date);
                command.Parameters.AddWithValue(
                    "@numberOfGuests", model.NumberOfGuests);
                command.Parameters.AddWithValue(
                    "@budget", model.Budget);
                command.Parameters.AddWithValue(
                    "@client_ID", model.Client_ID);
                command.Parameters.AddWithValue(
                    "@venueName", model.VenueName);

                await command.ExecuteNonQueryAsync();

                TempData["Success"] =
                    "Event and venue booked successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdowns();
                return View(model);
            }
        }

        private async Task LoadDropdowns()
        {
            var clients =
                new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();

            var venues =
                new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using (var command = new SqlCommand(
                "SELECT client_ID, cfname, clname FROM dbo.client",
                connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    clients.Add(
                        new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1) + " " +
                                   reader.GetString(2)
                        });
                }
            }

            using (var command = new SqlCommand(
                "SELECT venueName FROM dbo.venue",
                connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    venues.Add(
                        new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                        {
                            Value = reader.GetString(0),
                            Text = reader.GetString(0)
                        });
                }
            }

            ViewBag.Clients = clients;
            ViewBag.Venues = venues;
        }

    }
}
