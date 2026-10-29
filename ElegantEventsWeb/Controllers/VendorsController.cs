
using Microsoft.AspNetCore.Mvc;
using ElegantEventsWeb.Data;
using ElegantEventsWeb.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace ElegantEventsWeb.Controllers
{
    [Authorize(Roles = "Administrator,Coordinator")]
    public class VendorsController : Controller
    {
        private readonly DatabaseConnection _database;

        public VendorsController(DatabaseConnection database)
        {
            _database = database;
        }

        public async Task<IActionResult> Index()
        {
            var vendors = new List<Vendor>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT vEmail, vAddress, vFName, vLName,
                       contactPersonnel, vPhone, subvendorEmail
                FROM dbo.vendor
                ORDER BY vFName, vLName;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                vendors.Add(new Vendor
                {
                    VEmail = reader.GetString(0),
                    VAddress = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    VFName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    VLName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    ContactPersonnel = reader.IsDBNull(4)
                        ? "" : reader.GetString(4),
                    VPhone = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    SubvendorEmail = reader.IsDBNull(6)
                        ? null : reader.GetString(6)
                });
            }

            return View(vendors);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Vendor());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vendor vendor)
        {
            if (string.IsNullOrWhiteSpace(vendor.VEmail) ||
                string.IsNullOrWhiteSpace(vendor.VFName) ||
                string.IsNullOrWhiteSpace(vendor.VLName))
            {
                ModelState.AddModelError("",
                    "Vendor email, first name and last name are required.");

                return View(vendor);
            }

            if (!ModelState.IsValid)
                return View(vendor);

            try
            {
                using var connection = _database.GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO dbo.vendor
                (vEmail, vAddress, vFName, vLName,
                 contactPersonnel, vPhone, subvendorEmail)
            VALUES
                (@Email, @Address, @FirstName, @LastName,
                 @Contact, @Phone, @Subvendor);
        ";

                command.Parameters.AddWithValue("@Email", vendor.VEmail);
                command.Parameters.AddWithValue("@Address", vendor.VAddress);
                command.Parameters.AddWithValue("@FirstName", vendor.VFName);
                command.Parameters.AddWithValue("@LastName", vendor.VLName);
                command.Parameters.AddWithValue("@Contact", vendor.ContactPersonnel);
                command.Parameters.AddWithValue("@Phone", vendor.VPhone);
                command.Parameters.AddWithValue("@Subvendor",
                    string.IsNullOrWhiteSpace(vendor.SubvendorEmail)
                        ? DBNull.Value
                        : (object)vendor.SubvendorEmail);

                await command.ExecuteNonQueryAsync();

                TempData["Success"] = "Vendor added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                ModelState.AddModelError("",
                    ex.Number == 2627 || ex.Number == 2601
                        ? "A vendor with this email already exists."
                        : "Unable to save vendor. Check the information.");
                return View(vendor);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            Vendor? vendor = null;

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
        SELECT vEmail, vAddress, vFName, vLName,
               contactPersonnel, vPhone, subvendorEmail
        FROM dbo.vendor
        WHERE vEmail = @Email;
    ";
            command.Parameters.AddWithValue("@Email", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                vendor = new Vendor
                {
                    VEmail = reader.GetString(0),
                    VAddress = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    VFName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    VLName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    ContactPersonnel = reader.IsDBNull(4)
                        ? "" : reader.GetString(4),
                    VPhone = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    SubvendorEmail = reader.IsDBNull(6)
                        ? null : reader.GetString(6)
                };
            }

            if (vendor == null)
                return NotFound();

            return View(vendor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Vendor vendor)
        {
            if (id != vendor.VEmail)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(vendor.VFName) ||
                string.IsNullOrWhiteSpace(vendor.VLName))
            {
                ModelState.AddModelError("",
                    "First name and last name are required.");
                return View(vendor);
            }

            try
            {
                using var connection = _database.GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
            UPDATE dbo.vendor
            SET vAddress = @Address,
                vFName = @FirstName,
                vLName = @LastName,
                contactPersonnel = @Contact,
                vPhone = @Phone,
                subvendorEmail = @Subvendor
            WHERE vEmail = @Email;
        ";

                command.Parameters.AddWithValue("@Email", id);
                command.Parameters.AddWithValue("@Address", vendor.VAddress);
                command.Parameters.AddWithValue("@FirstName", vendor.VFName);
                command.Parameters.AddWithValue("@LastName", vendor.VLName);
                command.Parameters.AddWithValue("@Contact", vendor.ContactPersonnel);
                command.Parameters.AddWithValue("@Phone", vendor.VPhone);
                command.Parameters.AddWithValue("@Subvendor",
                    string.IsNullOrWhiteSpace(vendor.SubvendorEmail)
                        ? DBNull.Value
                        : (object)vendor.SubvendorEmail);

                int affected = await command.ExecuteNonQueryAsync();

                if (affected == 0)
                    return NotFound();

                TempData["Success"] = "Vendor updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Microsoft.Data.SqlClient.SqlException)
            {
                ModelState.AddModelError("",
                    "Unable to update this vendor.");
                return View(vendor);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            Vendor? vendor = null;

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
        SELECT vEmail, vFName, vLName
        FROM dbo.vendor
        WHERE vEmail = @Email;
    ";

            command.Parameters.AddWithValue("@Email", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                vendor = new Vendor
                {
                    VEmail = reader.GetString(0),
                    VFName = reader.GetString(1),
                    VLName = reader.GetString(2)
                };
            }

            if (vendor == null)
                return NotFound();

            return View(vendor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            try
            {
                using var connection = _database.GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();

                command.CommandText = @"
            DELETE FROM dbo.vendor
            WHERE vEmail = @Email;
        ";

                command.Parameters.AddWithValue("@Email", id);

                int affected = await command.ExecuteNonQueryAsync();

                if (affected == 0)
                    return NotFound();

                TempData["Success"] = "Vendor deleted successfully!";
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
                when (ex.Number == 547)
            {
                TempData["Error"] =
                    "Cannot delete this vendor because related records exist.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Contracts(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var contracts = new List<VendorContract>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(
                "dbo.SP_Vendor_Contracts", connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@vEmail", SqlDbType.VarChar, 50).Value = id;

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                contracts.Add(new VendorContract
                {
                    VEmail = reader.GetString(reader.GetOrdinal("vEmail")),
                    Event_ID = reader.GetString(reader.GetOrdinal("event_ID")).Trim(),
                    Event_Name = reader.GetString(reader.GetOrdinal("event_name")),
                    Event_Date = reader.GetDateTime(reader.GetOrdinal("event_Date")),
                    ContractDate = reader.GetDateTime(reader.GetOrdinal("contractDate")),
                    PaymentStatus = reader.GetString(reader.GetOrdinal("paymentStatus")),
                    Cost = reader.GetDecimal(reader.GetOrdinal("cost"))
                });
            }

            ViewBag.VendorEmail = id;
            return View(contracts);
        }

    }
}
