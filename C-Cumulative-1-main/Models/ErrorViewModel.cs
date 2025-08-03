////using System.Collections.Generic;
//using Microsoft.EntityFrameworkCore;
//using MyDbApp.Models; // Your model namespace

//namespace MyDbApp.Data
//{
//    public class AppDbContext : DbContext
//    {
//        public DbSet<Student> Students { get; set; }

//        protected override void OnConfiguring(DbContextOptionsBuilder options)
//        {
//            var connectionString = "server=localhost;database=school;user=root;password=;port=3306;";
//            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
//        }
//    }
//}