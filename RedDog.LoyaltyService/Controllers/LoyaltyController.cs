using System;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedDog.LoyaltyService.Models;

namespace RedDog.LoyaltyService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoyaltyController : ControllerBase
    {
        private const string OrderTopic = "orders";
        private const string PubSubName = "reddog.pubsub";
        private const string LoyaltyStateStoreName = "reddog.state.loyalty";
        private readonly ILogger<LoyaltyController> _logger;

        public LoyaltyController(ILogger<LoyaltyController> logger)
        {
            _logger = logger;
        }

        [Topic(PubSubName, OrderTopic)]
        [HttpPost("orders")]
        public async Task<IActionResult> UpdateLoyalty(OrderSummary orderSummary, [FromServices] DaprClient daprClient)
        {
            _logger.LogInformation("Received Order Summary: {@OrderSummary}", orderSummary);

            // TODO: Test if orderSummary.OrderTotal == null
            int loyaltyPointsEarned = (int)Math.Round(orderSummary.OrderTotal * 10, 0, MidpointRounding.AwayFromZero);

            StateEntry<LoyaltySummary> stateEntry = null;
            try
            {
                stateEntry = await daprClient.GetStateEntryAsync<LoyaltySummary>(LoyaltyStateStoreName, orderSummary.LoyaltyId);
                stateEntry.Value ??= new LoyaltySummary()
                {
                    FirstName = orderSummary.FirstName,
                    LastName = orderSummary.LastName,
                    LoyaltyId = orderSummary.LoyaltyId,
                    PointTotal = 0
                };
                stateEntry.Value.PointsEarned = loyaltyPointsEarned;
                stateEntry.Value.PointTotal += loyaltyPointsEarned;
                await stateEntry.SaveAsync();
                _logger.LogInformation("Successfully updated loyalty points: {@LoyaltySummary}", stateEntry.Value);
            }
            catch(Exception e)
            {
                _logger.LogError("Error saving loyalty summary: {@LoyaltySummary}, Message: {Message}", stateEntry.Value, e.Message);
            }

            return Ok(stateEntry.Value);
        }
    }
}
