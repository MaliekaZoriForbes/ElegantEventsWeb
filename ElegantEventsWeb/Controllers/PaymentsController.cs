
using ElegantEventsWeb.Data;
using ElegantEventsWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace ElegantEventsWeb.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class PaymentsController : Controller
    {
        private readonly DatabaseConnection _database;

        public PaymentsController(DatabaseConnection database)
        {
            _database = database;
        }

        public async Task<IActionResult> Index()
        {
            var payments = new List<Payment>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT paymentNumber, vEmail, event_ID,
                       contractDate, amountPaid,
                       balanceOutstanding, methodPay
                FROM dbo.payment
                ORDER BY paymentNumber DESC;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                payments.Add(new Payment
                {
                    PaymentNumber = reader.GetInt32(0),
                    VEmail = reader.GetString(1),
                    Event_ID = reader.GetString(2).Trim(),
                    ContractDate = reader.GetDateTime(3),
                    AmountPaid = reader.IsDBNull(4)
                        ? 0 : reader.GetDecimal(4),
                    BalanceOutstanding = reader.IsDBNull(5)
                        ? 0 : reader.GetDecimal(5),
                    MethodPay = reader.IsDBNull(6)
                        ? "" : reader.GetString(6)
                });
            }

            return View(payments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadContracts();
            return View(new Payment());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Payment model, string contractKey)
        {

            ModelState.Remove(nameof(Payment.VEmail));
            ModelState.Remove(nameof(Payment.Event_ID));

            if (model.AmountPaid <= 0)
            {
                ModelState.AddModelError(
                    nameof(Payment.AmountPaid),
                    "Payment amount must be greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                await LoadContracts();
                return View(model);
            }


            var parts = contractKey?.Split('|');

            if (parts == null || parts.Length != 3 ||
                !DateTime.TryParse(parts[2],
                    out var contractDate))
            {
                ModelState.AddModelError("",
                    "Select a valid contract.");
                await LoadContracts();
                return View(model);
            }

            try
            {
                using var connection = _database.GetConnection();
                await connection.OpenAsync();

                // Validate the contract exists.
                using var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = @"
                    SELECT COUNT(*)
                    FROM dbo.contract
                    WHERE vEmail = @Email
                      AND event_ID = @EventID
                      AND contractDate = @ContractDate;
                ";

                checkCommand.Parameters.Add("@Email",
                    SqlDbType.VarChar, 50).Value = parts[0];
                checkCommand.Parameters.Add("@EventID",
                    SqlDbType.Char, 10).Value = parts[1];
                checkCommand.Parameters.Add("@ContractDate",
                    SqlDbType.Date).Value = contractDate.Date;

                int count = Convert.ToInt32(
                    await checkCommand.ExecuteScalarAsync());

                if (count == 0)
                {
                    ModelState.AddModelError("",
                        "The selected contract no longer exists.");
                    await LoadContracts();
                    return View(model);
                }

                using var command = new SqlCommand(
                    "dbo.tr_record_payment", connection);

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@vEmail",
                    SqlDbType.VarChar, 50).Value = parts[0];

                command.Parameters.Add("@event_ID",
                    SqlDbType.Char, 10).Value = parts[1];

                command.Parameters.Add("@contractDate",
                    SqlDbType.Date).Value = contractDate.Date;

                var amount = command.Parameters.Add(
                    "@amountPaid", SqlDbType.Decimal);
                amount.Precision = 20;
                amount.Scale = 2;
                amount.Value = model.AmountPaid;

                command.Parameters.Add("@methodPay",
                    SqlDbType.VarChar, 15).Value = model.MethodPay;

                await command.ExecuteNonQueryAsync();

                TempData["Success"] =
                    "Payment recorded successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("",
                    "Payment rejected: " + ex.Message);

                await LoadContracts();
                return View(model);
            }
        }

        private async Task LoadContracts()
        {
            var contracts = new List<SelectListItem>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT vEmail, event_ID, contractDate,
                       dbo.fn_contract_balance(
                           vEmail, event_ID, contractDate
                       ) AS balance
                FROM dbo.contract
                ORDER BY contractDate DESC;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string email = reader.GetString(0);
                string eventId = reader.GetString(1).Trim();
                DateTime date = reader.GetDateTime(2);
                decimal balance = reader.GetDecimal(3);

                contracts.Add(new SelectListItem
                {
                    Value = email + "|" + eventId + "|" +
                            date.ToString("yyyy-MM-dd"),
                    Text = eventId + " - " + email +
                           " (Balance: JMD " +
                           balance.ToString("N2") + ")",
                    Disabled = balance <= 0
                });
            }

            ViewBag.Contracts = contracts;
        }
    }
}
