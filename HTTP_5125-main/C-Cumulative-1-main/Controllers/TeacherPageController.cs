using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SchoolDbcontext.Models;   // AccessDatabase()
using School.Models;            // Teacher

namespace School.Controllers
{
    public class TeacherPageController : Controller
    {
        private readonly SchoolDbContext _db = new SchoolDbContext();

        // ============ LIST ============
        [HttpGet("/Teacher")]
        public async Task<IActionResult> Index()
        {
            var list = new List<Teacher>();
            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            const string sql = @"SELECT teacherid, teacherfname, teacherlname, employeenumber, hiredate, salary, teacherworkphone
                                 FROM teachers ORDER BY teacherid";
            using var cmd = new MySqlCommand(sql, conn);
            using var rdr = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                int idOrd = rdr.GetOrdinal("teacherid");
                int fnOrd = rdr.GetOrdinal("teacherfname");
                int lnOrd = rdr.GetOrdinal("teacherlname");
                int empOrd = rdr.GetOrdinal("employeenumber");
                int hdOrd = rdr.GetOrdinal("hiredate");
                int salOrd = rdr.GetOrdinal("salary");
                int phOrd = rdr.GetOrdinal("teacherworkphone");

                list.Add(new Teacher
                {
                    TeacherId = rdr.GetInt32(idOrd),
                    TeacherFname = rdr.GetString(fnOrd),
                    TeacherLname = rdr.GetString(lnOrd),
                    EmployeeNumber = rdr.GetString(empOrd),
                    HireDate = rdr.GetDateTime(hdOrd),
                    Salary = rdr.IsDBNull(salOrd) ? 0M : Convert.ToDecimal(rdr.GetValue(salOrd)),
                    TeacherWorkPhone = rdr.IsDBNull(phOrd) ? null : rdr.GetString(phOrd)
                });
            }
            return View("Index", list);
        }

        // ============ DETAILS ============
        [HttpGet("/Teacher/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            const string sql = @"SELECT teacherid, teacherfname, teacherlname, employeenumber, hiredate, salary, teacherworkphone
                                 FROM teachers WHERE teacherid=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rdr = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            if (!await rdr.ReadAsync()) return NotFound();

            int idOrd = rdr.GetOrdinal("teacherid");
            int fnOrd = rdr.GetOrdinal("teacherfname");
            int lnOrd = rdr.GetOrdinal("teacherlname");
            int empOrd = rdr.GetOrdinal("employeenumber");
            int hdOrd = rdr.GetOrdinal("hiredate");
            int salOrd = rdr.GetOrdinal("salary");
            int phOrd = rdr.GetOrdinal("teacherworkphone");

            var t = new Teacher
            {
                TeacherId = rdr.GetInt32(idOrd),
                TeacherFname = rdr.GetString(fnOrd),
                TeacherLname = rdr.GetString(lnOrd),
                EmployeeNumber = rdr.GetString(empOrd),
                HireDate = rdr.GetDateTime(hdOrd),
                Salary = rdr.IsDBNull(salOrd) ? 0M : Convert.ToDecimal(rdr.GetValue(salOrd)),
                TeacherWorkPhone = rdr.IsDBNull(phOrd) ? null : rdr.GetString(phOrd)
            };
            return View("Details", t);
        }

        // ============ NEW ============
        [HttpGet("/Teacher/New")]
        public IActionResult New() => View("New", new Teacher());

        [ValidateAntiForgeryToken]
        [HttpPost("/Teacher/New")]
        public async Task<IActionResult> New(Teacher t)
        {
            if (!ModelState.IsValid) return View("New", t);

            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            const string sql = @"INSERT INTO teachers (teacherfname, teacherlname, employeenumber, hiredate, salary, teacherworkphone)
                                 VALUES (@fn, @ln, @emp, @hd, @sal, @phone)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@fn", t.TeacherFname);
            cmd.Parameters.AddWithValue("@ln", t.TeacherLname);
            cmd.Parameters.AddWithValue("@emp", t.EmployeeNumber);
            cmd.Parameters.AddWithValue("@hd", t.HireDate);
            cmd.Parameters.AddWithValue("@sal", t.Salary);
            cmd.Parameters.AddWithValue("@phone", (object?)t.TeacherWorkPhone ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            TempData[rows > 0 ? "Success" : "Error"] = rows > 0 ? "Teacher added." : "Insert failed.";
            return RedirectToAction("Index");
        }

        // ============ EDIT ============
        [HttpGet("/Teacher/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            const string sql = @"SELECT teacherid, teacherfname, teacherlname, employeenumber, hiredate, salary, teacherworkphone
                                 FROM teachers WHERE teacherid=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rdr = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            if (!await rdr.ReadAsync()) return NotFound();

            int idOrd = rdr.GetOrdinal("teacherid");
            int fnOrd = rdr.GetOrdinal("teacherfname");
            int lnOrd = rdr.GetOrdinal("teacherlname");
            int empOrd = rdr.GetOrdinal("employeenumber");
            int hdOrd = rdr.GetOrdinal("hiredate");
            int salOrd = rdr.GetOrdinal("salary");
            int phOrd = rdr.GetOrdinal("teacherworkphone");

            var t = new Teacher
            {
                TeacherId = rdr.GetInt32(idOrd),
                TeacherFname = rdr.GetString(fnOrd),
                TeacherLname = rdr.GetString(lnOrd),
                EmployeeNumber = rdr.GetString(empOrd),
                HireDate = rdr.GetDateTime(hdOrd),
                Salary = rdr.IsDBNull(salOrd) ? 0M : Convert.ToDecimal(rdr.GetValue(salOrd)),
                TeacherWorkPhone = rdr.IsDBNull(phOrd) ? null : rdr.GetString(phOrd)
            };
            return View("Edit", t);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("/Teacher/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, Teacher form)
        {
            if (id != form.TeacherId)
            {
                ModelState.AddModelError(string.Empty, "Route id and form TeacherId do not match.");
                return View("Edit", form);
            }
            if (!ModelState.IsValid) return View("Edit", form);

            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            const string sql = @"UPDATE teachers SET
                                    teacherfname=@fn,
                                    teacherlname=@ln,
                                    employeenumber=@emp,
                                    hiredate=@hd,
                                    salary=@sal,
                                    teacherworkphone=@phone
                                 WHERE teacherid=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@fn", form.TeacherFname);
            cmd.Parameters.AddWithValue("@ln", form.TeacherLname);
            cmd.Parameters.AddWithValue("@emp", form.EmployeeNumber);
            cmd.Parameters.AddWithValue("@hd", form.HireDate);
            cmd.Parameters.AddWithValue("@sal", form.Salary);
            cmd.Parameters.AddWithValue("@phone", (object?)form.TeacherWorkPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            TempData[rows > 0 ? "Success" : "Error"] = rows > 0 ? "Teacher updated." : "Update failed.";
            return RedirectToAction("Details", new { id = form.TeacherId });
        }

        // ============ DELETE ============
        [HttpGet("/Teacher/Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            const string sql = @"SELECT teacherid, teacherfname, teacherlname, employeenumber, hiredate, salary, teacherworkphone
                                 FROM teachers WHERE teacherid=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var rdr = (MySqlDataReader)await cmd.ExecuteReaderAsync();
            Teacher? t = null;
            if (await rdr.ReadAsync())
            {
                int idOrd = rdr.GetOrdinal("teacherid");
                int fnOrd = rdr.GetOrdinal("teacherfname");
                int lnOrd = rdr.GetOrdinal("teacherlname");
                int empOrd = rdr.GetOrdinal("employeenumber");
                int hdOrd = rdr.GetOrdinal("hiredate");
                int salOrd = rdr.GetOrdinal("salary");
                int phOrd = rdr.GetOrdinal("teacherworkphone");

                t = new Teacher
                {
                    TeacherId = rdr.GetInt32(idOrd),
                    TeacherFname = rdr.GetString(fnOrd),
                    TeacherLname = rdr.GetString(lnOrd),
                    EmployeeNumber = rdr.GetString(empOrd),
                    HireDate = rdr.GetDateTime(hdOrd),
                    Salary = rdr.IsDBNull(salOrd) ? 0M : Convert.ToDecimal(rdr.GetValue(salOrd)),
                    TeacherWorkPhone = rdr.IsDBNull(phOrd) ? null : rdr.GetString(phOrd)
                };
            }
            return View("Delete", t); // View handles null state
        }

        [ValidateAntiForgeryToken]
        [HttpPost("/Teacher/Delete/{id:int}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var conn = _db.AccessDatabase();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("DELETE FROM teachers WHERE teacherid=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var rows = await cmd.ExecuteNonQueryAsync();

            TempData[rows > 0 ? "Success" : "Error"] = rows > 0 ? "Teacher deleted." : "Teacher not found.";
            return RedirectToAction("Index");
        }
    }
}
