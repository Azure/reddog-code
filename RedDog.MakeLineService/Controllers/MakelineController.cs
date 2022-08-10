using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
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

        [HttpPost("/orderCreated")]
        public async Task<IActionResult> AddOrderToMakeLine(OrderSummary orderSummary)
        {
            _logger.LogInformation("[OrderCreatedEvent] Received Order: {@OrderSummary}", orderSummary);

            try
            {
                await _daprClient.SaveStateAsync<OrderSummary>(MakeLineStateStoreName, orderSummary.OrderId.ToString(), orderSummary, _stateOptions, new Dictionary<string, string>{{ "ttlInSeconds", "60" }});
                _logger.LogInformation("Successfully added Order to Make Line: {OrderId}", orderSummary.OrderId);
            }
            catch(Exception e)
            {
                _logger.LogError("Error saving order summaries. Message: {Content}", e.InnerException?.Message ?? e.Message);
                return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
            }

            return Ok();
        }


        [HttpPost("/orderStatusChangedInProgress")]
        public async Task<IActionResult> OnOrderStatusChangedCreated(OrderStatusChanged orderStatusChanged)
        {
            _logger.LogInformation("[OrderStatusChangedEvent=InProgress] Order: {OrderId}", orderStatusChanged.OrderId);

            bool isSuccess;

            do
            {
                var stateEntry = await _daprClient.GetStateEntryAsync<OrderSummary>(MakeLineStateStoreName, orderStatusChanged.OrderId.ToString());

                if(stateEntry.Value == null)
                {
                    // TODO: What happens if TTL were to delete the state entry before this message was received?
                    _logger.LogWarning("Unable to find existing order for OrderId: {orderId}", orderStatusChanged.OrderId);
                    return Problem($"Unable to find existing order for OrderId: {orderStatusChanged.OrderId}");
                }

                if(stateEntry.Value.OrderStatus < orderStatusChanged.OrderStatus)
                {
                    stateEntry.Value.OrderStatus = orderStatusChanged.OrderStatus;
                    isSuccess = await stateEntry.TrySaveAsync(_stateOptions, new Dictionary<string, string>{{ "ttlInSeconds", "60" }});
                    _logger.LogInformation("Successfully updated order status for order: {OrderId}", orderStatusChanged.OrderId);
                }
                else
                {
                    isSuccess = true;
                    _logger.LogInformation("OrderStatusChanged event is stale. Skipping status change for order: {OrderId}", orderStatusChanged.OrderId);
                }
                
            } while(!isSuccess);

            return Ok();
        }

        [HttpPost("/orderStatusChangedCompleted")]
        public async Task<IActionResult> OnOrderStatusChangedCompleted(OrderStatusChanged orderStatusChanged)
        {
            _logger.LogInformation("[OrderStatusChangedEvent=Completed] Order: {OrderId}", orderStatusChanged.OrderId);
            
            bool isSuccess;
            StateEntry<OrderSummary> stateEntry;

            do
            {
                stateEntry = await _daprClient.GetStateEntryAsync<OrderSummary>(MakeLineStateStoreName, orderStatusChanged.OrderId.ToString());
                
                if(stateEntry.Value == null)
                {
                    // TODO: What happens if TTL were to delete the state entry before this message was received?
                    _logger.LogWarning("Unable to find existing order for OrderId: {orderId}", orderStatusChanged.OrderId);
                    return Problem($"Unable to find existing order for OrderId: {orderStatusChanged.OrderId}", null, (int)HttpStatusCode.InternalServerError);
                }

                if(stateEntry.Value.OrderStatus < orderStatusChanged.OrderStatus)
                {
                    stateEntry.Value.OrderStatus = orderStatusChanged.OrderStatus;
                    stateEntry.Value.OrderCompletedDate = DateTime.UtcNow;
                    isSuccess = await stateEntry.TrySaveAsync(_stateOptions, new Dictionary<string, string>{{ "ttlInSeconds", "60" }});
                    _logger.LogInformation("Successfully updated order status for order: {OrderId}", orderStatusChanged.OrderId);
                }
                else
                {
                    isSuccess = true;
                    _logger.LogInformation("OrderStatusChanged event is stale. Skipping status change for order: {OrderId}", orderStatusChanged.OrderId);
                }
                
            } while(!isSuccess);

            try
            {
                var cloudEvent = new CloudEvent<OrderSummary>(stateEntry.Value) { Type = OrderCompletedEventType };
                await _daprClient.PublishEventAsync<CloudEvent<OrderSummary>>(PubSubName, OrderTopic, cloudEvent);
                _logger.LogInformation("Published order completed message for OrderId: {orderId}", orderStatusChanged.OrderId);
            }
            catch(Exception e)
            {
                _logger.LogError("Error publishing order completed message for OrderId: {orderId}. Message: {Content}", orderStatusChanged.OrderId, e.InnerException?.Message ?? e.Message);
                return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
            }

            return Ok();
        }

        private async Task<StateEntry<List<OrderSummary>>> GetAllOrdersAsync(string storeId)
        {
            return await _daprClient.GetStateEntryAsync<List<OrderSummary>>(MakeLineStateStoreName, storeId);
        }
    }
}
