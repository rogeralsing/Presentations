using Microsoft.AspNetCore.Mvc;
using Microphone;
using System.Threading.Tasks;
using System.Net.Http;

namespace Service1.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET: api/values
        [HttpGet]
        public async Task<string> Get([FromServices]IClusterClient client)
        {            
            var uri = await client.ResolveUriAsync("service2","/api/hello");

            var http = new HttpClient();
            http.BaseAddress = uri;
            var res = await http.GetStringAsync("");
            return res;
        }
    }
}
