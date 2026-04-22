using Microsoft.AspNetCore.Mvc;


namespace YourAPP_Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Hello, World!";
        }
    }
}
