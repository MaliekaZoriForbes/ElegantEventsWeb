
using Microsoft.AspNetCore.Mvc;
using ElegantEventsWeb.Data;
using ElegantEventsWeb.Models;
using Microsoft.AspNetCore.Authorization;
namespace ElegantEventsWeb.Controllers
{
    [Authorize(Roles = "Administrator,Coordinator")]
    public class ClientsController : Controller
    {
        private readonly DatabaseConnection _database;

        public ClientsController(DatabaseConnection database)
        {
            _database = database;
        }

        public async Task<IActionResult> Index()
        {
            var clients = new List<Client>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT client_ID, cfname, clname,
                       phone, email, billingAddress
                FROM dbo.client
                ORDER BY client_ID;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clients.Add(new Client
                {
                    Client_ID = reader.GetInt32(0),
                    Cfname = reader.GetString(1),
                    Clname = reader.GetString(2),
                    Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    BillingAddress = reader.IsDBNull(5)
                        ? "" : reader.GetString(5)
                });
            }

            return View(clients);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Client());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.Cfname) ||
                string.IsNullOrWhiteSpace(client.Clname))
            {
                ModelState.AddModelError("",
                    "First name and last name are required.");

                return View(client);
            }

            try
            {
                using var connection = _database.GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();

                command.CommandText = @"
            INSERT INTO dbo.client
                (cfname, clname, phone, email, billingAddress)
            VALUES
                (@FirstName, @LastName, @Phone, @Email, @Address);
        ";

                command.Parameters.AddWithValue("@FirstName", client.Cfname);
                command.Parameters.AddWithValue("@LastName", client.Clname);
                command.Parameters.AddWithValue("@Phone", client.Phone);
                command.Parameters.AddWithValue("@Email", client.Email);
                command.Parameters.AddWithValue("@Address", client.BillingAddress);

                await command.ExecuteNonQueryAsync();

                TempData["Success"] = "Client added successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                ModelState.AddModelError("",
                    "Unable to save the client. Check the information and try again.");
                return View(client);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Client? client = null;

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
        SELECT client_ID, cfname, clname
        FROM dbo.client
        WHERE client_ID = @Id;
    ";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                client = new Client
                {
                    Client_ID = reader.GetInt32(0),
                    Cfname = reader.GetString(1),
                    Clname = reader.GetString(2)
                };
            }

            if (client == null)
                return NotFound();

            return View(client);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                using var connection = _database.GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
            DELETE FROM dbo.client
            WHERE client_ID = @Id;
        ";
                command.Parameters.AddWithValue("@Id", id);

                int affected = await command.ExecuteNonQueryAsync();

                if (affected == 0)
                    return NotFound();

                TempData["Success"] = "Client deleted successfully!";
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
                when (ex.Number == 547)
            {
                TempData["Error"] =
                    "This client cannot be deleted because they have related records.";
            }

            return RedirectToAction(nameof(Index));
        }

        // EDIT CLIENT - Display the edit form
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Client? client = null;

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT client_ID, cfname, clname,
                       phone, email, billingAddress
                FROM dbo.client
                WHERE client_ID = @Id;
            ";

            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                client = new Client
                {
                    Client_ID = reader.GetInt32(0),
                    Cfname = reader.GetString(1),
                    Clname = reader.GetString(2),
                    Phone = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Email = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    BillingAddress = reader.IsDBNull(5)
                        ? "" : reader.GetString(5)
                };
            }

            if (client == null)
                return NotFound();

            return View(client);
        }

        // EDIT CLIENT - Save updated information
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.Client_ID)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(client.Cfname) ||
                string.IsNullOrWhiteSpace(client.Clname))
            {
                ModelState.AddModelError("",
                    "First name and last name are required.");

                return View(client);
            }

            try
            {
                using var connection = _database.GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();

                command.CommandText = @"
                    UPDATE dbo.client
                    SET cfname = @FirstName,
                        clname = @LastName,
                        phone = @Phone,
                        email = @Email,
                        billingAddress = @Address
                    WHERE client_ID = @Id;
                ";

                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@FirstName", client.Cfname);
                command.Parameters.AddWithValue("@LastName", client.Clname);
                command.Parameters.AddWithValue("@Phone", client.Phone ?? "");
                command.Parameters.AddWithValue("@Email", client.Email ?? "");
                command.Parameters.AddWithValue("@Address",
                    client.BillingAddress ?? "");

                int affected = await command.ExecuteNonQueryAsync();

                if (affected == 0)
                    return NotFound();

                TempData["Success"] = "Client updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                ModelState.AddModelError("",
                    "Unable to update this client.");

                return View(client);
            }
        }


    }
}
