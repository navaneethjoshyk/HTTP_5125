using System;                        // Convert.ToDecimal / Convert.ToInt32
using System.Diagnostics;           // Debug.WriteLine
using System.Text.Json;             // payload logging
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SchoolDbcontext.Models;       // AccessDatabase()
using School.Models;                // Teacher model

namespace School.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/Teacher")]
    public class TeacherAPIController : ControllerBase
    {
        private readonly SchoolDbContext _db = new SchoolDbContext();

        // Employee number must be: 'T' followed by digits (e.g., T5005)
        private static readonly Regex EmpRegex = new(@"^T\d+$", RegexOptions.Compiled);

        // ===================== Helpers =====================

        private static Teacher MapReader(MySqlDataReader r)
        {
            // Use ordinals everywhere (MySqlDataReader typed getters take column indexes)
            int idOrd = r.GetOrdinal("teacherid");
            int fnOrd = r.GetOrdinal("teacherfname");
            int lnOrd = r.GetOrdinal("teacherlname");
            int empOrd = r.GetOrdinal("employeenumber");
            int hdOrd = r.GetOrdinal("hiredate");
            int salOrd = r.GetOrdinal("salary");
            int phoneOrd = r.GetOrdinal("teacherworkphone");

            return new Teacher
            {
                TeacherId = r.GetInt32(idOrd),
                TeacherFname = r.GetString(fnOrd),
                TeacherLname = r.GetString(lnOrd),
                EmployeeNumber = r.GetString(empOrd),
                HireDate = r.GetDateTime(hdOrd),
                // Convert.ToDecimal is extra-safe for MySQL DECIMAL types
                Salary = r.IsDBNull(salOrd) ? 0M : Convert.ToDecimal(r.GetValue(salOrd)),
                TeacherWorkPhone = r.IsDBNull(phoneOrd) ? null : r.GetString(phoneOrd)
            };
        }

        private async Task<bool> EmployeeNumberExistsAsync(string emp, int? excludeId = null)
        {
            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            string sql = excludeId.HasValue
                ? @"SELECT 1 FROM teachers WHERE employeenumber=@emp AND teacherid<>@id LIMIT 1"
                : @"SELECT 1 FROM teachers WHERE employeenumber=@emp LIMIT 1";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@emp", emp);
            if (excludeId.HasValue) cmd.Parameters.AddWithValue("@id", excludeId.Value);

            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }

        private static (bool ok, string? msg) ValidateTeacher(Teacher t, bool requireIdMatch = false, int? routeId = null)
        {
            if (requireIdMatch && routeId.HasValue && t.TeacherId != routeId.Value)
                return (false, "Route id and body.TeacherId must match.");
            if (string.IsNullOrWhiteSpace(t.TeacherFname))
                return (false, "First name is required.");
            if (string.IsNullOrWhiteSpace(t.TeacherLname))
                return (false, "Last name is required.");
            if (!EmpRegex.IsMatch(t.EmployeeNumber))
                return (false, "Employee number must start with 'T' followed by digits (e.g., T5005).");
            if (t.HireDate.Date > DateTime.UtcNow.Date)
                return (false, "Hire date cannot be in the future.");
            if (t.Salary < 0)
                return (false, "Salary must be >= 0.");
            return (true, null);
        }

        // ===================== READ =====================

        /// <summary>Get a single teacher by id.</summary>
        /// <param name="id">TeacherId</param>
        /// <returns>200 with Teacher; 404 if not found.</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Teacher), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Teacher>> GetById(int id)
        {
            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            const string sql = @"SELECT teacherid, teacherfname, teacherlname, employeenumber, hiredate, salary, teacherworkphone
                                 FROM teachers WHERE teacherid=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rdrObj = await cmd.ExecuteReaderAsync();
            var rdr = (MySqlDataReader)rdrObj;
            if (!await rdr.ReadAsync())
                return NotFound(new { message = "Teacher not found." });

            return Ok(MapReader(rdr));
        }

        // ===================== EXISTS (client validation support) =====================

        /// <summary>Check if an employee number already exists.</summary>
        /// <param name="empNo">Employee number (e.g., T5005)</param>
        [HttpGet("exists/empno/{empNo}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> ExistsEmpNo(string empNo)
            => Ok(await EmployeeNumberExistsAsync(empNo));

        // ===================== ADD =====================

        /// <summary>Add a new teacher.</summary>
        /// <param name="model">Teacher payload</param>
        /// <remarks>
        /// Request header: <c>Content-Type: application/json</c><br/>
        /// Example JSON:
        /// {
        ///   "teacherId": 0,
        ///   "teacherFname": "Alex",
        ///   "teacherLname": "Morgan",
        ///   "employeeNumber": "T5005",
        ///   "hireDate": "2022-09-01",
        ///   "salary": 62000,
        ///   "teacherWorkPhone": "416-555-0123 x204"
        /// }
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(Teacher), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Add([FromBody] Teacher model)
        {
            Debug.WriteLine($"[ADD] {JsonSerializer.Serialize(model)}");

            var v = ValidateTeacher(model);
            if (!v.ok) return BadRequest(new { message = v.msg });

            if (await EmployeeNumberExistsAsync(model.EmployeeNumber))
                return Conflict(new { message = "Employee number already exists." });

            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            const string sql = @"INSERT INTO teachers 
                (teacherfname, teacherlname, employeenumber, hiredate, salary, teacherworkphone)
                VALUES (@fn, @ln, @emp, @hd, @sal, @phone);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@fn", model.TeacherFname);
            cmd.Parameters.AddWithValue("@ln", model.TeacherLname);
            cmd.Parameters.AddWithValue("@emp", model.EmployeeNumber);
            cmd.Parameters.AddWithValue("@hd", model.HireDate);
            cmd.Parameters.AddWithValue("@sal", model.Salary);
            cmd.Parameters.AddWithValue("@phone", (object?)model.TeacherWorkPhone ?? DBNull.Value);

            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            model.TeacherId = newId;

            return CreatedAtAction(nameof(GetById), new { id = newId }, model);
        }

        // ===================== UPDATE =====================

        /// <summary>Update an existing teacher.</summary>
        /// <param name="id">Route TeacherId</param>
        /// <param name="model">Teacher payload (TeacherId must match route)</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Update(int id, [FromBody] Teacher model)
        {
            Debug.WriteLine($"[UPDATE] id={id} {JsonSerializer.Serialize(model)}");

            var v = ValidateTeacher(model, requireIdMatch: true, routeId: id);
            if (!v.ok) return BadRequest(new { message = v.msg });

            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            // Ensure exists
            using (var check = new MySqlCommand("SELECT 1 FROM teachers WHERE teacherid=@id", conn))
            {
                check.Parameters.AddWithValue("@id", id);
                var existsObj = await check.ExecuteScalarAsync();
                if (existsObj is null)
                    return NotFound(new { message = "Teacher not found." });
            }

            // Duplicate EmployeeNumber (excluding this record)?
            if (await EmployeeNumberExistsAsync(model.EmployeeNumber, excludeId: id))
                return Conflict(new { message = "Employee number already exists." });

            const string sql = @"UPDATE teachers SET
                    teacherfname=@fn,
                    teacherlname=@ln,
                    employeenumber=@emp,
                    hiredate=@hd,
                    salary=@sal,
                    teacherworkphone=@phone
                 WHERE teacherid=@id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@fn", model.TeacherFname);
            cmd.Parameters.AddWithValue("@ln", model.TeacherLname);
            cmd.Parameters.AddWithValue("@emp", model.EmployeeNumber);
            cmd.Parameters.AddWithValue("@hd", model.HireDate);
            cmd.Parameters.AddWithValue("@sal", model.Salary);
            cmd.Parameters.AddWithValue("@phone", (object?)model.TeacherWorkPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);

            await cmd.ExecuteNonQueryAsync();
            return NoContent();
        }

        // ===================== DELETE =====================

        /// <summary>Delete a teacher.</summary>
        /// <param name="id">TeacherId to delete</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            Debug.WriteLine($"[DELETE] id={id}");

            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            // If you don't have DB-level FK ON DELETE SET NULL on related tables,
            // you can manually detach here before deleting:
            // using (var detach = new MySqlCommand("UPDATE courses SET teacherid=NULL WHERE teacherid=@id", conn))
            // { detach.Parameters.AddWithValue("@id", id); await detach.ExecuteNonQueryAsync(); }

            using var cmd = new MySqlCommand("DELETE FROM teachers WHERE teacherid=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var affected = await cmd.ExecuteNonQueryAsync();

            if (affected == 0)
                return NotFound(new { message = "Teacher not found." });

            return NoContent();
        }
    }
}
