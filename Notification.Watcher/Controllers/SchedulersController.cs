using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.AgentSmith.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulersController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public SchedulersController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var schedulersList = await _schedulerFactory.GetAllSchedulers();

                return Ok(schedulersList);
            }
            catch (Exception ex)
            {
                return base.Problem(ex.Message);
            }
            
        }
    }
}
