using Microsoft.AspNetCore.Mvc;

namespace CSharpAssignment1.Controllers
{
    [ApiController]
    [Route("api/q5")]
    public class Q5Controller : ControllerBase
    {
        [HttpPost("secret")]
        public string PostSecret([FromBody] int secret)
        {
            return $"Shh.. the secret is {secret}";
        }
    }
}