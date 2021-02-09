using Microsoft.AspNetCore.Mvc;
using Notification.Data.Context;
using Notification.Data.Model;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Notification.Data.Dto;
using Notification.Brain.Services;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Notification.Brain.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly NotificationContext _context;
        private readonly CampaignService _campaignService;

        public CampaignController(NotificationContext context, CampaignService campaignService)
        {
            _context = context;
            _campaignService = campaignService;

            //_hubConnection = new HubConnectionBuilder()
            //    .WithUrl("http://localhost:5000/notification", option => { option.UseDefaultCredentials = true; })
            //    .WithAutomaticReconnect()
            //    .ConfigureLogging(factory =>
            //    {
            //        factory.AddConsole();
            //        factory.AddFilter("Console", level => level >= LogLevel.Trace);
            //    }).Build();

            //_hubConnection.StartAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("All")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<CampaignDto>))]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Campaigns.AsNoTracking().ToListAsync());
        }

        [HttpPost]
        //[Authorize()]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CampaignResponseDto))]
        public async Task<IActionResult> Post([FromBody] CampaignDto campaign)
        {
            // TODO : check if all data in campaign are ok
            //if (message != null && !string.IsNullOrEmpty(toUser) && !string.IsNullOrEmpty(fromUser))
            //{
            //    await _hubConnection.SendAsync("SendToUser", message, toUser);
            //    //await _hubConnection.SendAsync("Send", new Message { Name = name, Text = message });
            //    //logger.LogInformation("SendAsync to Hub");
            //}
            //else
            //{
            //    return base.ValidationProblem("a parameter is missing");
            //}
            try
            {
                var camp = campaign.ToCampaign();
                camp.IsActive = true;
                _context.Campaigns.Add(camp);
                _context.Messages.Add(camp.Message);
                foreach (var usr in camp.Recepients)
                {
                    var dbUsr = _context.UsersChat.FirstOrDefault(e => e.Name == usr.Name);
                    if (dbUsr == null)
                    {
                        _context.UsersChat.Add(usr);
                    }
                    //if (dbUsr != null)
                    //{
                    //    _context.Entry(dbUsr).State = EntityState.Modified;
                    //}
                    //else
                    //{
                    //    _context.Entry(usr).State = EntityState.Added;
                    //}
                }

                await _context.SaveChangesAsync();

                // TODO : call check campaign method from scheduler with id
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:5001/api/NotificationScheduler/Jobs/");

                    HttpResponseMessage response = await client.PostAsJsonAsync("Notifications/SimpleTrigger", new Campaign() { ID = camp.ID, StartDate = camp.StartDate, EndDate = camp.EndDate });

                    // TODO: Rollback database if API error

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    
                    //TODO: manage result
                }

                //await _campaignService.CheckForOpenCampaign(); // not used, made by scheduler, replace by scheduler call to create triggers
            }
            catch (Exception exc)
            {
                // TODO : log error
                
                return base.Problem(exc.Message);
            }

            // TODO : Send ID in response
            return Ok();
        }

        [HttpPut]
        //[Authorize()]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CampaignResponseDto))]
        public async Task<IActionResult> Put(int campaignID, [FromBody] CampaignDto campaign)
        {
            // TODO : check if all data in campaign are ok
            //if (message != null && !string.IsNullOrEmpty(toUser) && !string.IsNullOrEmpty(fromUser))
            //{
            //    await _hubConnection.SendAsync("SendToUser", message, toUser);
            //    //await _hubConnection.SendAsync("Send", new Message { Name = name, Text = message });
            //    //logger.LogInformation("SendAsync to Hub");
            //}
            //else
            //{
            //    return base.ValidationProblem("a parameter is missing");
            //}
            try
            {
                var camp = campaign.ToCampaign();
                var c = _context.Campaigns.FirstOrDefault(p => p.ID == campaignID);

                c.StartDate = campaign.StartDate;
                c.EndDate = campaign.EndDate;
                c.Message = campaign.Message.ToMessage();

                await _context.SaveChangesAsync();

                // TODO : call check campaign method from scheduler with id


            }
            catch (Exception exc)
            {
                // TODO : log error
                return base.Problem(exc.Message);
            }

            // TODO : Send ID in response
            return Ok();
        }

        [HttpDelete]
        //[Authorize()]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(int campaignID)
        {
            // TODO : check if all data in campaign are ok
            //if (message != null && !string.IsNullOrEmpty(toUser) && !string.IsNullOrEmpty(fromUser))
            //{
            //    await _hubConnection.SendAsync("SendToUser", message, toUser);
            //    //await _hubConnection.SendAsync("Send", new Message { Name = name, Text = message });
            //    //logger.LogInformation("SendAsync to Hub");
            //}
            //else
            //{
            //    return base.ValidationProblem("a parameter is missing");
            //}
            try
            {
                var campaign = _context.Campaigns.FirstOrDefault(p=>p.ID == campaignID);

                if (campaign == null) return NotFound(campaignID);

                campaign.EndDate = DateTime.Now;
                campaign.IsActive = false;

                await _context.SaveChangesAsync();

                // TODO : refresh caches
            }
            catch (Exception exc)
            {
                // TODO : log error
                return base.Problem(exc.Message);
            }

            return Ok(campaignID);
        }
    }
}
