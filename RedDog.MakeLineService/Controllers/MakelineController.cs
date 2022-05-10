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
        private const string OrderCompletedTopic = "ordercompleted";
        private const string PubSubName = "reddog.pubsub";
        private const string MakeLineStateStoreName = "reddog.state.makeline";
        private readonly ILogger<MakelineController> _logger;
        private readonly DaprClient _daprClient;
        private readonly StateOptions _stateOptions = new StateOptions(){ Concurrency = ConcurrencyMode.FirstWrite, Consistency = ConsistencyMode.Eventual };

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

            StateEntry<List<OrderSummary>> state = null;
            try
            {
                bool isSuccess;

                do
                {
                    state = await _daprClient.GetStateEntryAsync<List<OrderSummary>>(MakeLineStateStoreName, orderSummary.StoreId);
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

        [HttpGet("/orders/{storeId}")]
        public async Task<IActionResult> GetOrders(string storeId)
        {
            var orders = (await GetAllOrders(storeId)).Value ?? new List<OrderSummary>();
            return new OkObjectResult(orders.OrderBy(o => o.OrderDate));
        }

        [HttpDelete("/orders/{storeId}/{orderId}")]
        public async Task<IActionResult> CompleteOrder(string storeId, Guid orderId)
        {
            _logger.LogInformation("Completing Order: {OrderId}", orderId);

            bool isSuccess;
            OrderSummary order;
            DateTime orderCompletedDate = DateTime.UtcNow;

            var orders = await GetAllOrders(storeId);
            order = orders.Value.FirstOrDefault(o => o.OrderId == orderId);
            order.OrderCompletedDate = orderCompletedDate;
            
            if (order != null)
            {
                try
                {
                    await _daprClient.PublishEventAsync<OrderSummary>(PubSubName, OrderCompletedTopic, order);
                    _logger.LogInformation("Published order completed message for OrderId: {orderId}");
                }
                catch(Exception e)
                {
                    _logger.LogError("Error publishing order completed message for OrderId: {orderId}. Message: {Content}", e.InnerException?.Message ?? e.Message);
                    return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
                }
            }

            do
            {
                if (order != null)
                {
                    orders.Value.Remove(order);
                    try
                    {
                        isSuccess = await orders.TrySaveAsync(_stateOptions);
                        if(!isSuccess)
                        {
                            orders = await GetAllOrders(storeId);
                            order = orders.Value.FirstOrDefault(o => o.OrderId == orderId);
                            order.OrderCompletedDate = orderCompletedDate;
                        }
                    }
                    catch(Exception e)
                    {
                        _logger.LogError("Error saving order summaries. Message: {Content}", e.InnerException?.Message ?? e.Message);
                        return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    isSuccess = true;
                }
            }
            while(!isSuccess);

            _logger.LogInformation("Completed Order: {@OrderSummary}", order);

            return new OkResult();
        }

        private async Task<StateEntry<List<OrderSummary>>> GetAllOrders(string storeId)
        {
            return await _daprClient.GetStateEntryAsync<List<OrderSummary>>(MakeLineStateStoreName, storeId);
        }
    }
}
