using Microsoft.AspNetCore.Mvc;

namespace CSharpAssignment1.Controllers
{
    [ApiController]
    [Route("api/q3")]
    public class Q3Controller : ControllerBase
    {
        [HttpGet("cube/{baseNum}")]
        public int GetCube(int baseNum)
        {
            return baseNum * baseNum * baseNum;
        }
    }
}