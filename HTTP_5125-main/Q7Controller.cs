using Microsoft.AspNetCore.Mvc;
using System;

namespace CSharpAssignment1.Controllers
{
    [ApiController]
    [Route("api/q7")]
    public class Q7Controller : ControllerBase
    {
        [HttpGet("timemachine")]
        public string GetAdjustedDate([FromQuery] int days)
        {
            return DateTime.Today.AddDays(days).ToString("yyyy-MM-dd");
        }
    }
}