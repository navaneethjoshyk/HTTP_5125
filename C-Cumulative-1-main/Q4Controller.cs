using Microsoft.AspNetCore.Mvc;

namespace CSharpAssignment1.Controllers
{
    [ApiController]
    [Route("api/q4")]
    public class Q4Controller : ControllerBase
    {
        [HttpPost("knockknock")]
        public string StartJoke()
        {
            return "Who's there?";
        }
    }
}