
using ElegantEventsWeb.Data;
using ElegantEventsWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace ElegantEventsWeb.Controllers
{
    [Authorize(Roles = "Administrator,Coordinator")]
    public class StaffController : Controller
    {
        private readonly DatabaseConnection _database;

        public StaffController(DatabaseConnection database)
        {
            _database = database;
        }

        public async Task<IActionResult> Index()
        {
            var staffList = new List<Staff>();

            using var connection = _database.GetConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT empNumber, dataHired, fname,
                       mname, lname, phone, salary
                FROM dbo.staff
                ORDER BY fname, lname;
            ";

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                staffList.Add(new Staff
                {
                    EmpNumber = reader.GetString(0).Trim(),
                    DateHired = reader.GetDateTime(1),
                    Fname = reader.GetString(2),
                    Mname = reader.IsDBNull(3)
                        ? "" : reader.GetString(3),
                    Lname = reader.GetString(4),
                    Phone = reader.IsDBNull(5)
                        ? "" : reader.GetString(5),
                    Salary = reader.GetDecimal(6)
                });
            }

            return View(staffList);
        }
        
[HttpGet]
public async Task<IActionResult> Assignments()
{
    var assignments = new List<StaffAssignment>();

    using var connection = _database.GetConnection();
    await connection.OpenAsync();

    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT w.empNumber, w.event_ID, w.rolee,
               s.fname, s.lname, e.event_name
        FROM dbo.works w
        JOIN dbo.staff s ON w.empNumber = s.empNumber
        JOIN dbo.eventt e ON w.event_ID = e.event_ID
        ORDER BY w.event_ID;
    ";

    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        assignments.Add(new StaffAssignment
        {
            EmpNumber = reader.GetString(0).Trim(),
            Event_ID = reader.GetString(1).Trim(),
            Role = reader.GetString(2),
            StaffName = reader.GetString(3) + " " +
                        reader.GetString(4),
            EventName = reader.GetString(5)
        });
    }

    return View(assignments);
}

[HttpGet]
public async Task<IActionResult> Assign()
{
    await LoadAssignmentDropdowns();
    return View(new StaffAssignment());
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Assign(StaffAssignment model)
{
    if (string.IsNullOrWhiteSpace(model.EmpNumber) ||
        string.IsNullOrWhiteSpace(model.Event_ID) ||
        string.IsNullOrWhiteSpace(model.Role) ||
        model.Role.Length > 15)
    {
        ModelState.AddModelError("",
            "Select a staff member, event and valid role.");

        await LoadAssignmentDropdowns();
        return View(model);
    }

    try
    {
        using var connection = _database.GetConnection();
        await connection.OpenAsync();

        using var command = new SqlCommand(
            "dbo.SP_Staff_Event_Assign", connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add("@empNumber",
            SqlDbType.Char, 7).Value = model.EmpNumber;


                command.Parameters.Add("@event_ID",
                    SqlDbType.Char, 10).Value = model.Event_ID;

                command.Parameters.Add("@rolee",
                    SqlDbType.VarChar, 15).Value = model.Role;

                await command.ExecuteNonQueryAsync();

        TempData["Success"] =
            "Staff assigned to event successfully!";

        return RedirectToAction(nameof(Assignments));
    }
    catch (SqlException ex)
    {
        ModelState.AddModelError("",
            ex.Number == 2627 || ex.Number == 2601
                ? "This staff member is already assigned to that event."
                : "Assignment failed: " + ex.Message);

        await LoadAssignmentDropdowns();
        return View(model);
    }
}

private async Task LoadAssignmentDropdowns()
{
    var staffOptions = new List<SelectListItem>();
    var eventOptions = new List<SelectListItem>();

    using var connection = _database.GetConnection();
    await connection.OpenAsync();

    using (var command = new SqlCommand(
        "SELECT empNumber, fname, lname FROM dbo.staff",
        connection))
    using (var reader = await command.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            staffOptions.Add(new SelectListItem
            {
                Value = reader.GetString(0).Trim(),
                Text = reader.GetString(1) + " " +
                       reader.GetString(2)
            });
        }
    }

    using (var command = new SqlCommand(
        "SELECT event_ID, event_name FROM dbo.eventt",
        connection))
    using (var reader = await command.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            eventOptions.Add(new SelectListItem
            {
                Value = reader.GetString(0).Trim(),
                Text = reader.GetString(1) + " (" +
                       reader.GetString(0).Trim() + ")"
            });
        }
    }

    ViewBag.StaffOptions = staffOptions;
    ViewBag.EventOptions = eventOptions;
}

    }
}

