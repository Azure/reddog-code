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
        private readonly DaprClient _daprClient;

        public LoyaltyController(ILogger<LoyaltyController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [Topic(PubSubName, OrderTopic)]
        [HttpPost("orders")]
        public async Task<IActionResult> UpdateLoyalty(OrderSummary orderSummary)
        {
            _logger.LogInformation("Received Order Summary: {@OrderSummary}", orderSummary);

            // TODO: Test if orderSummary.OrderTotal == null
            int loyaltyPointsEarned = (int)Math.Round(orderSummary.OrderTotal * 10, 0, MidpointRounding.AwayFromZero);

            StateEntry<LoyaltySummary> stateEntry = null;
            try
            {
                stateEntry = await _daprClient.GetStateEntryAsync<LoyaltySummary>(LoyaltyStateStoreName, orderSummary.LoyaltyId);
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
                _logger.LogError("Error saving loyalty summary: {@LoyaltySummary}, Message: {Message}", stateEntry.Value, e.InnerException?.Message ?? e.Message);
            }

            return Ok(stateEntry.Value);
        }
    }
}
