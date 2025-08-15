using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace School.Models // <-- you can pick "School" or your project name
{
    /// <summary>
    /// Represents a teacher in the system.
    /// </summary>
    /// <remarks>
    /// <para><b>Example JSON (POST/PUT):</b></para>
    /// <code>
    /// {
    ///   "teacherId": 0,
    ///   "teacherFname": "Alex",
    ///   "teacherLname": "Morgan",
    ///   "employeeNumber": "T5005",
    ///   "hireDate": "2022-09-01",
    ///   "salary": 62000.00,
    ///   "teacherWorkPhone": "416-555-0123 x204"
    /// }
    /// </code>
    /// </remarks>
    [Table("teachers")]
    public class Teacher
    {
        [Key]
        [Column("teacherid")]
        public int TeacherId { get; set; }

        [Required, StringLength(100)]
        [Column("teacherfname")]
        public string TeacherFname { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Column("teacherlname")]
        public string TeacherLname { get; set; } = string.Empty;

        /// <summary>
        /// Must start with 'T' followed by digits (e.g., T5005). Unique per database.
        /// </summary>
        [Required, StringLength(32)]
        [RegularExpression(@"^T\d+$", ErrorMessage = "Employee number must start with 'T' followed by digits (e.g., T5005).")]
        [Column("employeenumber")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Column("hiredate", TypeName = "date")]
        public DateTime HireDate { get; set; }

        [Range(0, 999_999_999)]
        [Column("salary", TypeName = "decimal(10,2)")]
        public decimal Salary { get; set; }

        /// <summary>
        /// Free-form work phone (extensions allowed). Not strictly validated to avoid false rejects.
        /// </summary>
        [StringLength(255)]
        [Column("teacherworkphone")]
        public string? TeacherWorkPhone { get; set; }
    }
}