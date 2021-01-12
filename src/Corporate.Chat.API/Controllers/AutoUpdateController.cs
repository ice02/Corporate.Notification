using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Corporate.Chat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoUpdateController : ControllerBase
    {
        // GET api/messages
        [HttpGet("")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(List<string>))]
        public async Task<IActionResult> Get()
        {
            // TODO : get username to profile
            // TODO : Put in a database or in a cache based on real binaries
            return Ok(new List<string>() { "1.0 https://localhost:5001/1.0.zip", "2.0 https://localhost:5001/2.0.zip" });
        }
    }
}
