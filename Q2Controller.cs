using Microsoft.AspNetCore.Mvc;

namespace CSharpAssignment1.Controllers
{
    [ApiController]
    [Route("api/q2")]
    public class Q2Controller : ControllerBase
    {
        /// <summary>
        /// Returns a greeting message for the given name.
        /// </summary>
        /// <param name="name">Name to greet</param>
        /// <returns>Greeting message</returns>
        /// <example>GET /api/q2/greeting?name=Gary -> "Hi Gary!"</example>
        [HttpGet("greeting")]
        public string GetGreeting([FromQuery] string name)
        {
            return $"Hi {name}!";
        }
    }
}