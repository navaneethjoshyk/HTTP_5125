using Microsoft.AspNetCore.Mvc;
using School.Models;
using SchoolDbcontext.Models;
using System;
using System.Text.RegularExpressions;

namespace School.Controllers
{
    public class TeacherPageController : Controller
    {
        private readonly SchoolDbContext _context = new SchoolDbContext();

        // GET: /TeacherPage/New
        public IActionResult New()
        {
            return View(new Teacher());
        }

        // POST: /TeacherPage/New
        [HttpPost]
        public IActionResult New(Teacher newTeacher)
        {
            Console.WriteLine("🟡 POST /TeacherPage/New hit");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Invalid Model State");
                return View(newTeacher);
            }

            try
            {
                using (var conn = _context.AccessDatabase())
                {
                    conn.Open();
                    Console.WriteLine("✅ DB Connection Open");

                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"INSERT INTO teachers 
                (teacherfname, teacherlname, hiredate, salary, employeenumber, teacherworkphone) 
                VALUES (@fname, @lname, @hiredate, @salary, @emp, @phone)";
                    cmd.Parameters.AddWithValue("@fname", newTeacher.TeacherFName);
                    cmd.Parameters.AddWithValue("@lname", newTeacher.TeacherLName);
                    cmd.Parameters.AddWithValue("@hiredate", newTeacher.HireDate);
                    cmd.Parameters.AddWithValue("@salary", newTeacher.Salary);
                    cmd.Parameters.AddWithValue("@emp", newTeacher.EmployeeNumber);
                    cmd.Parameters.AddWithValue("@phone", newTeacher.TeacherWorkPhone ?? "");

                    int inserted = cmd.ExecuteNonQuery();
                    Console.WriteLine($"📥 Rows Inserted: {inserted}");

                    if (inserted > 0)
                    {
                        TempData["Success"] = "✅ Teacher added successfully!";
                        return RedirectToAction("New");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Insert failed.");
                        return View(newTeacher);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 ERROR: " + ex.Message);
                ModelState.AddModelError("", "Server error: " + ex.Message);
                return View(newTeacher);
            }
        }
    }
}