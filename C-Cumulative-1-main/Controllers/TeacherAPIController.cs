using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using School.Models;
using SchoolDbcontext.Models;
using System;
using System.Collections.Generic;

namespace School.Controllers
{
    [Route("api/teacher")]
    [ApiController]
    public class TeacherAPIController : ControllerBase
    {
        private readonly SchoolDbContext _context = new SchoolDbContext();

        /// <summary>
        /// Returns a list of all teacher details
        /// </summary>
        /// <example>GET api/teacher/GetAllTeachers</example>
        [HttpGet("GetAllTeachers")]
        public ActionResult<List<Teacher>> GetAllTeachers()
        {
            List<Teacher> teacherList = new List<Teacher>();

            try
            {
                using (MySqlConnection connection = _context.AccessDatabase())
                {
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM teachers";

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Teacher teacher = new Teacher()
                            {
                                TeacherId = Convert.ToInt32(reader["teacherid"]),
                                TeacherFName = reader["teacherfname"].ToString(),
                                TeacherLName = reader["teacherlname"].ToString(),
                                HireDate = Convert.ToDateTime(reader["hiredate"]),
                                Salary = Convert.ToDecimal(reader["salary"])
                            };

                            teacherList.Add(teacher);
                        }
                    }
                }

                return Ok(teacherList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns details of a single teacher based on ID
        /// </summary>
        /// <example>GET api/teacher/FindTeacher?id=3</example>
        [HttpGet("FindTeacher")]
        public ActionResult<Teacher> FindTeacher([FromQuery] int id)
        {
            try
            {
                using (MySqlConnection connection = _context.AccessDatabase())
                {
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM teachers WHERE teacherid = @id";
                    command.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Teacher teacher = new Teacher()
                            {
                                TeacherId = Convert.ToInt32(reader["teacherid"]),
                                TeacherFName = reader["teacherfname"].ToString(),
                                TeacherLName = reader["teacherlname"].ToString(),
                                HireDate = Convert.ToDateTime(reader["hiredate"]),
                                Salary = Convert.ToDecimal(reader["salary"])
                            };

                            return Ok(teacher);
                        }
                        else
                        {
                            return NotFound($"No teacher found with ID {id}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new teacher to the database
        /// </summary>
        /// <example>POST api/teacher/AddTeacher</example>
        [HttpPost("AddTeacher")]
        public ActionResult AddTeacher([FromBody] Teacher newTeacher)
        {
            if (newTeacher == null)
            {
                return BadRequest("Teacher data is required.");
            }

            if (string.IsNullOrWhiteSpace(newTeacher.TeacherFName) || string.IsNullOrWhiteSpace(newTeacher.TeacherLName))
            {
                return BadRequest("Teacher first and last name are required.");
            }

            if (newTeacher.HireDate > DateTime.Today)
            {
                return BadRequest("Hire date cannot be in the future.");
            }

            try
            {
                using (MySqlConnection connection = _context.AccessDatabase())
                {
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO teachers (teacherfname, teacherlname, hiredate, salary)
                                            VALUES (@fname, @lname, @hiredate, @salary)";
                    command.Parameters.AddWithValue("@fname", newTeacher.TeacherFName);
                    command.Parameters.AddWithValue("@lname", newTeacher.TeacherLName);
                    command.Parameters.AddWithValue("@hiredate", newTeacher.HireDate);
                    command.Parameters.AddWithValue("@salary", newTeacher.Salary);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        return Ok("Teacher added successfully.");
                    }
                    else
                    {
                        return StatusCode(500, "Error adding teacher.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a teacher from the database by ID
        /// </summary>
        /// <example>DELETE api/teacher/DeleteTeacher?id=3</example>
        [HttpDelete("DeleteTeacher")]
        public ActionResult DeleteTeacher([FromQuery] int id)
        {
            try
            {
                using (MySqlConnection connection = _context.AccessDatabase())
                {
                    connection.Open();

                    // Check if teacher exists
                    MySqlCommand checkCmd = connection.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM teachers WHERE teacherid = @id";
                    checkCmd.Parameters.AddWithValue("@id", id);

                    long count = (long)checkCmd.ExecuteScalar();
                    if (count == 0)
                    {
                        return NotFound($"No teacher found with ID {id}");
                    }

                    // Proceed to delete
                    MySqlCommand deleteCmd = connection.CreateCommand();
                    deleteCmd.CommandText = "DELETE FROM teachers WHERE teacherid = @id";
                    deleteCmd.Parameters.AddWithValue("@id", id);

                    int result = deleteCmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        return Ok($"Teacher with ID {id} deleted successfully.");
                    }
                    else
                    {
                        return StatusCode(500, "Error deleting teacher.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
