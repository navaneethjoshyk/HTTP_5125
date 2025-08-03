using System;
using System.ComponentModel.DataAnnotations;

namespace School.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name can't be longer than 50 characters.")]
        [Display(Name = "First Name")]
        public string TeacherFName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name can't be longer than 50 characters.")]
        [Display(Name = "Last Name")]
        public string TeacherLName { get; set; }

        [Required(ErrorMessage = "Hire date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; }

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0, 1000000, ErrorMessage = "Salary must be between 0 and 1,000,000.")]
        [Display(Name = "Salary")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Employee number is required.")]
        [RegularExpression(@"^T\d+$", ErrorMessage = "Employee Number must start with 'T' followed by digits (e.g., T123).")]
        [Display(Name = "Employee Number")]
        public string EmployeeNumber { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [Display(Name = "Work Phone")]
        public string TeacherWorkPhone { get; set; }
    }
}
