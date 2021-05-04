using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedDog.MakeLineService.Models;

namespace RedDog.MakeLineService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MakelineController : ControllerBase
    {
        private const string OrderTopic = "orders";
        private const string PubSubName = "reddog.pubsub";
        private const string MakeLineStateStoreName = "reddog.state.makeline";
        private readonly ILogger<MakelineController> _logger;
        private readonly DaprClient _daprClient;

        public MakelineController(ILogger<MakelineController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [Topic(PubSubName, OrderTopic)]
        [HttpPost("/orders")]
        public async Task<IActionResult> AddOrderToMakeLine(OrderSummary orderSummary)
        {
            _logger.LogInformation("Received Order: {@OrderSummary}", orderSummary);

            try
            {
                var state = await _daprClient.GetStateEntryAsync<List<OrderSummary>>(MakeLineStateStoreName, orderSummary.StoreId);
                state.Value ??= new List<OrderSummary>();
                state.Value.Add(orderSummary);
                await state.SaveAsync();

                _logger.LogInformation("Successfully added Order to Make Line: {OrderId}", orderSummary.OrderId);
            }
            catch(Exception e)
            {
                _logger.LogError("Error saving order summaries. Message: {Content}", e.Message);
                return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
            }

            return new OkResult();
        }

        [HttpGet("/orders/{storeId}")]
        public async Task<IActionResult> GetOrders(string storeId)
        {
            var orders = await GetAllOrders(storeId);
            return new OkObjectResult(orders.OrderBy(o => o.OrderDate));
        }

        [HttpDelete("/orders/{storeId}/{orderId}")]
        public async Task<IActionResult> CompleteOrder(string storeId, Guid orderId)
        {
            _logger.LogInformation("Completing Order: {OrderId}", orderId);

            var orders = await GetAllOrders(storeId);

            // Look up the order and remove it from the store
            var order = orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                orders.Remove(order);
                try
                {
                    await _daprClient.SaveStateAsync<List<OrderSummary>>(MakeLineStateStoreName, storeId, orders);
                    _logger.LogInformation("Completed Order: {@OrderSummary}", order);
                }
                catch(Exception e)
                {
                    _logger.LogError("Error saving order summaries. Message: {Content}", e.Message);
                    return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
                }
            }

            return new OkResult();
        }

        private async Task<List<OrderSummary>> GetAllOrders(string storeId)
        {
            var state = await _daprClient.GetStateEntryAsync<List<OrderSummary>>(MakeLineStateStoreName, storeId);
            return state.Value;
        }
    }
}
