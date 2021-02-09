using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification.Data.Context;
using Notification.Data.Model;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;

namespace Notification.Engine.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly NotificationContext context;
        private readonly IClientList _ClientLists;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ClientLists"></param>
        public UsersController(NotificationContext context, IClientList ClientLists)
        {
            this.context = context;
            _ClientLists = ClientLists;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET api/users
        [HttpGet("{userName}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<NotificationUser>))]
        public async Task<IActionResult> GetByName(string userName)
        {
            return Ok(await context.UsersChat.Where(p=>p.Name.ToUpper() == userName.ToUpper()).AsNoTracking().ToListAsync());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET api/users
        [HttpGet("Connected")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<Client>))]
        public IActionResult GetClients()
        {
            return Ok(_ClientLists.GetClients());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        // GET api/users?page={0}&pageSize={1}
        [HttpGet("page={page}&pageSize={pageSize}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PagedResult<NotificationUser>))]
        public async Task<IActionResult> GetPaged([FromQuery] int page, int pageSize)
        {
            return Ok(await context.UsersChat.AsNoTracking().GetPagedAsync(page, pageSize));
        }
    }
}