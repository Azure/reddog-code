using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RedDog.AccountingModel;

namespace RedDog.AccountingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProbesController : ControllerBase
    {
        private static bool isReady;
        private ILogger<ProbesController> _logger;

        public ProbesController(ILogger<ProbesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("ready")]
        public async Task<IActionResult> IsReady([FromServices] AccountingContext dbContext)
        {
            if(!isReady)
            {
                try
                {
                    if(await dbContext.Orders.CountAsync() >= 0 && 
                       await dbContext.OrderItems.CountAsync() >= 0 &&
                       await dbContext.Customers.CountAsync() >= 0)
                    {
                        isReady = true;
                    }
                }
                catch(Exception e)
                {
                    _logger.LogWarning(e, "Readiness probe failure.");
                }

                return new StatusCodeResult(503);
            }

            return Ok();
        }
    }
}