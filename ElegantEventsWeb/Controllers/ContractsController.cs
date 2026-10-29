
using ElegantEventsWeb.Data;
using ElegantEventsWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using Microsoft.AspNetCore.Authorization;
namespace ElegantEventsWeb.Controllers
{
    [Authorize(Roles = "Administrator,Coordinator")]
    public class ContractsController : Controller
    {
        private readonly DatabaseConnection _database;

        public ContractsController(DatabaseConnection database)
        {
            _database = database;
        }

        public async Task<IActionResult> Index()
        {
            var contracts = new List<Contract>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT c.vEmail,
                       c.event_ID,
                       c.contractDate,
                       c.paymentStatus,
                       c.cost,
                       dbo.fn_contract_balance(
                           c.vEmail,
                           c.event_ID,
                           c.contractDate
                       ) AS outstanding_balance,
                       e.event_name
                FROM dbo.contract AS c
                INNER JOIN dbo.eventt AS e
                    ON c.event_ID = e.event_ID
                ORDER BY c.contractDate DESC;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                contracts.Add(new Contract
                {
                    VEmail = reader.GetString(0),
                    Event_ID = reader.GetString(1).Trim(),
                    ContractDate = reader.GetDateTime(2),
                    PaymentStatus = reader.GetString(3),
                    Cost = reader.GetDecimal(4),
                    OutstandingBalance = reader.GetDecimal(5),
                    EventName = reader.GetString(6)
                });
            }

            return View(contracts);
        }
    
    
[HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadContractDropdowns();

            return View(new Contract
            {
                ContractDate = DateTime.Today,
                PaymentStatus = "Outstanding"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract model)
        {
            if (string.IsNullOrWhiteSpace(model.VEmail) ||
                string.IsNullOrWhiteSpace(model.Event_ID) ||
                model.Cost <= 0 ||
                model.ContractDate == default)
            {
                ModelState.AddModelError("",
                    "Select a vendor, event, contract date and positive cost.");

                await LoadContractDropdowns();
                return View(model);
            }

            try
            {
                using var connection = _database.GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();

                command.CommandText = @"
            IF EXISTS (
                SELECT 1 FROM dbo.contract
                WHERE vEmail = @Email
                  AND event_ID = @EventID
                  AND contractDate = @ContractDate
            )
                THROW 50020, 'This contract already exists.', 1;

            INSERT INTO dbo.contract
                (vEmail, event_ID, contractDate,
                 paymentStatus, cost)
            VALUES
                (@Email, @EventID, @ContractDate,
                 'Outstanding', @Cost);
        ";

                command.Parameters.Add("@Email",
                    SqlDbType.VarChar, 50).Value = model.VEmail;

                command.Parameters.Add("@EventID",
                    SqlDbType.Char, 10).Value = model.Event_ID;

                command.Parameters.Add("@ContractDate",
                    SqlDbType.Date).Value = model.ContractDate.Date;

                var costParameter =
                    command.Parameters.Add("@Cost",
                        SqlDbType.Decimal);

                costParameter.Precision = 20;
                costParameter.Scale = 2;
                costParameter.Value = model.Cost;

                await command.ExecuteNonQueryAsync();

                TempData["Success"] = "Contract created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("",
                    ex.Number == 50020 || ex.Number == 2627 ||
                    ex.Number == 2601
                        ? "This contract already exists."
                        : ex.Number == 547
                            ? "The selected vendor or event does not exist."
                            : "Unable to create contract: " + ex.Message);

                await LoadContractDropdowns();
                return View(model);
            }
        }

        private async Task LoadContractDropdowns()
        {
            var vendors = new List<SelectListItem>();
            var events = new List<SelectListItem>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using (var command = new SqlCommand(
                "SELECT vEmail, vFName, vLName FROM dbo.vendor ORDER BY vFName",
                connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    vendors.Add(new SelectListItem
                    {
                        Value = reader.GetString(0),
                        Text = reader.GetString(1) + " " +
                               reader.GetString(2) + " (" +
                               reader.GetString(0) + ")"
                    });
                }
            }

            using (var command = new SqlCommand(
                "SELECT event_ID, event_name FROM dbo.eventt ORDER BY event_Date",
                connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    events.Add(new SelectListItem
                    {
                        Value = reader.GetString(0).Trim(),
                        Text = reader.GetString(1) + " (" +
                               reader.GetString(0).Trim() + ")"
                    });
                }
            }

            ViewBag.Vendors = vendors;
            ViewBag.Events = events;
        }

    }
}
