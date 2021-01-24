using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Corporate.Chat.API.Context;
using Corporate.Chat.API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Corporate.Chat.API.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ChatContext context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public UsersController(ChatContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET api/users
        [HttpGet("All")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<UserChat>))]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await context.UsersChat.AsNoTracking().ToListAsync());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        // GET api/users?page={0}&pageSize={1}
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedResult<UserChat>))]
        public async Task<IActionResult> GetPaged([FromQuery] int page, int pageSize)
        {
            return Ok(await context.UsersChat.AsNoTracking().GetPagedAsync(page, pageSize));
        }
    }
}