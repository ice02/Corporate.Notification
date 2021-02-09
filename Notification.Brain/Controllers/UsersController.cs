using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification.Data.Context;
using Notification.Data.Model;
using Swashbuckle.AspNetCore.Annotations;

namespace Notification.Brain.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly NotificationContext context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public UsersController(NotificationContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET api/users
        [HttpGet("All")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<NotificationUser>))]
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
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedResult<NotificationUser>))]
        public async Task<IActionResult> GetPaged([FromQuery] int page, int pageSize)
        {
            return Ok(await context.UsersChat.AsNoTracking().GetPagedAsync(page, pageSize));
        }
    }
}