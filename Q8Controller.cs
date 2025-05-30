using Microsoft.AspNetCore.Mvc;
using System;

namespace CSharpAssignment1.Controllers
{
    [ApiController]
    [Route("api/q8")]
    public class Q8Controller : ControllerBase
    {
        [HttpPost("squashfellows")]
        public string CalculateOrder([FromForm] int Small, [FromForm] int Large)
        {
            double smallTotal = Small * 25.50;
            double largeTotal = Large * 40.50;
            double subtotal = smallTotal + largeTotal;
            double tax = Math.Round(subtotal * 0.13, 2);
            double total = subtotal + tax;

            return $"{Small} Small @ $25.50 = ${smallTotal:0.00}; {Large} Large @ $40.50 = ${largeTotal:0.00}; " +
                   $"Subtotal = ${subtotal:0.00}; Tax = ${tax:0.00} HST; Total = ${total:0.00}";
        }
    }
}