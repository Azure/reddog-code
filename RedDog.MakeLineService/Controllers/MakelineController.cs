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
        private const string OrderCreatedEventType = "com.microsoft.reddog.ordercreated";
        private const string OrderStatusChangedEventType = "com.microsoft.reddog.orderstatuschanged";
        private const string OrderCompletedEventType = "com.microsoft.reddog.ordercompleted";
        private const string MakeLineStateStoreName = "reddog.state.makeline";
        private readonly ILogger<MakelineController> _logger;
        private readonly DaprClient _daprClient;
        private readonly StateOptions _stateOptions = new StateOptions(){ Concurrency = ConcurrencyMode.FirstWrite, Consistency = ConsistencyMode.Eventual };

        public MakelineController(ILogger<MakelineController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [HttpGet("/orders/{storeId}")]
        public async Task<IActionResult> GetOrders(string storeId)
        {
            var orders = (await GetAllOrdersAsync(storeId)).Value ?? new List<OrderSummary>();
            return new OkObjectResult(orders.OrderBy(o => o.OrderDate));
        }

        [Topic(PubSubName, OrderTopic, $"event.type == \"{OrderCreatedEventType}\"", 1)]
        [HttpPost("/orders")]
        public async Task<IActionResult> AddOrderToMakeLine(OrderSummary orderSummary)
        {
            _logger.LogInformation("Received Order: {@OrderSummary}", orderSummary);

            StateEntry<List<OrderSummary>> state = null;
            try
            {
                bool isSuccess;

                do
                {
                    state = await GetAllOrdersAsync(orderSummary.StoreId);
                    state.Value ??= new List<OrderSummary>();
                    state.Value.Add(orderSummary);
                    isSuccess = await state.TrySaveAsync(_stateOptions);
                } while(!isSuccess);

                _logger.LogInformation("Successfully added Order to Make Line: {OrderId}", orderSummary.OrderId);
            }
            catch(Exception e)
            {
                _logger.LogError("Error saving order summaries. Message: {Content}", e.InnerException?.Message ?? e.Message);
                return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
            }

            return new OkResult();
        }

        [Topic(PubSubName, OrderTopic, $"event.type == \"{OrderStatusChangedEventType}\"", 2)]
        [HttpPost("/orderCompleted")]
        public async Task<IActionResult> CompleteOrder(OrderStatusChanged orderStatusChanged)
        {
            _logger.LogInformation("Completing Order: {OrderId}", orderStatusChanged.OrderId);

            bool isSuccess;
            DateTime orderCompletedDate = DateTime.UtcNow;

            var orders = await GetAllOrdersAsync("Redmond");
            var order = orders?.Value.FirstOrDefault(o => o.OrderId == orderStatusChanged.OrderId);

            if(order == null)
            {
                _logger.LogWarning("Unable to find existing order for OrderId: {orderId}", orderStatusChanged.OrderId);
                return new OkObjectResult(new { status = "RETRY"});
            }
            

            order.OrderCompletedDate = orderCompletedDate;

            try
            {
                var cloudEvent = new CloudEvent<OrderSummary>(order) { Type = OrderCompletedEventType };
                await _daprClient.PublishEventAsync<CloudEvent<OrderSummary>>(PubSubName, OrderTopic, cloudEvent);
                _logger.LogInformation("Published order completed message for OrderId: {orderId}", order.OrderId);
            }
            catch(Exception e)
            {
                _logger.LogError("Error publishing order completed message for OrderId: {orderId}. Message: {Content}", order.OrderId, e.InnerException?.Message ?? e.Message);
                return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
            }


            do
            {
                orders.Value.Remove(order);
                try
                {
                    isSuccess = await orders.TrySaveAsync(_stateOptions);
                    if(!isSuccess)
                    {
                        orders = await GetAllOrdersAsync("Redmond");
                        order = orders.Value.FirstOrDefault(o => o.OrderId == order.OrderId);
                        order.OrderCompletedDate = orderCompletedDate;
                    }
                }
                catch(Exception e)
                {
                    _logger.LogError("Error saving order summaries. Message: {Content}", e.InnerException?.Message ?? e.Message);
                    return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
                }

            }
            while(!isSuccess);

            _logger.LogInformation("Completed Order: {@OrderSummary}", order);

            return new OkResult();
        }

        private async Task<StateEntry<List<OrderSummary>>> GetAllOrdersAsync(string storeId)
        {
            return await _daprClient.GetStateEntryAsync<List<OrderSummary>>(MakeLineStateStoreName, storeId);
        }
    }
}
