using Microsoft.AspNetCore.Mvc;
using System;

namespace CSharpAssignment1.Controllers
{
    [ApiController]
    [Route("api/q6")]
    public class Q6Controller : ControllerBase
    {
        [HttpGet("hexagon")]
        public double GetHexagonArea([FromQuery] double side)
        {
            return (3 * Math.Sqrt(3) / 2) * Math.Pow(side, 2);
        }
    }
}