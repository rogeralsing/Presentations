using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microphone;
using System.Linq;

namespace Service2.Controllers
{
    [Route("api/[controller]")]
    public class HelloController : Controller
    {
        [HttpGet()]
        public string Get()
        {
            return "hello from Service2";
        }
    }
}
