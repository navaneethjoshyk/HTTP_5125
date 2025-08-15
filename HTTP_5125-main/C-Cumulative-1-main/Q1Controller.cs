using Microsoft.AspNetCore.Mvc;

namespace CSharpAssignment1.Controllers
{
    [ApiController]
    [Route("api/q1")]
    public class Q1Controller : ControllerBase
    {
        /// <summary>
        /// Returns a welcome message
        /// </summary>
        /// <returns>String message</returns>
        /// <example>GET /api/q1/welcome -> "Welcome to 5125!"</example>
        [HttpGet("welcome")]
        public string GetWelcome()
        {
            return "Welcome to 5125!";
        }
    }
}