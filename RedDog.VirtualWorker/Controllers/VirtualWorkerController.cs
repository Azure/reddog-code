using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedDog.VirtualWorker.Models;

namespace RedDog.VirtualWorker.Controllers
{
    [ApiController]
    public class VirtualWorkerController : ControllerBase
    {
        private const string PubSubName = "reddog.pubsub";
        private const string OrderTopic = "orders";
        private const string OrderCreatedEventType = "com.microsoft.reddog.ordercreated";
        private const string OrderStatusChangedEventType = "com.microsoft.reddog.orderstatuschanged";
        public static readonly string StoreId = Environment.GetEnvironmentVariable("STORE_ID") ?? "Redmond";
        public static readonly int MinSecondsToCompleteItem = int.Parse(Environment.GetEnvironmentVariable("MIN_SECONDS_TO_COMPLETE_ITEM") ?? "1");
        public static readonly int MaxSecondsToCompleteItem = int.Parse(Environment.GetEnvironmentVariable("MAX_SECONDS_TO_COMPLETE_ITEM") ?? "5");
        private readonly ILogger<VirtualWorkerController> _logger;
        private readonly DaprClient _daprClient;
        private readonly Random _random;

        public VirtualWorkerController(ILogger<VirtualWorkerController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
            _random = new Random();
        }

        //[Topic(PubSubName, OrderTopic, $"event.type == \"{OrderCreatedEventType}\"", 1)]
        [Route("/orderCreated")]
        public async Task<IActionResult> MakeOrder(OrderSummary orderSummary)
        {
            _logger.LogInformation($"The VirtualWorker ({StoreId}) is making an order for {orderSummary.FirstName} {orderSummary.LastName}...");

            await UpdateOrderStatus(orderSummary, OrderStatus.InProgress);

            foreach (var orderItem in orderSummary.OrderItems)
            {
                _logger.LogInformation($"The VirtualWorker ({StoreId}) is making {orderItem.Quantity} {orderItem.ProductName}.");

                await Task.Delay(_random.Next(MinSecondsToCompleteItem * 1000, MaxSecondsToCompleteItem * 1000));

                _logger.LogInformation($"The VirtualWorker ({StoreId}) completed {orderItem.Quantity} {orderItem.ProductName}.");
            }

            await UpdateOrderStatus(orderSummary, OrderStatus.Completed);

            _logger.LogInformation($"{orderSummary.FirstName} {orderSummary.LastName}, your order is ready!");

            return Ok();
        }

        private async Task UpdateOrderStatus(OrderSummary orderSummary, OrderStatus orderStatus)
        {
            var orderStatusChanged = new OrderStatusChanged() { OrderId = orderSummary.OrderId, OrderStatus = orderStatus };

            try
            {
                var cloudEvent = new CloudEvent<OrderStatusChanged>(orderStatusChanged) { Type = OrderStatusChangedEventType };
                await _daprClient.PublishEventAsync<CloudEvent<OrderStatusChanged>>(PubSubName, OrderTopic, cloudEvent);
                _logger.LogInformation("Published Order Status Change: {@OrderSatusChanged}", orderStatusChanged);
            }
            catch(Exception e)
            {
                _logger.LogError("Error publishing Order Status Change: {@OrderStatusChanged}, Message: {Message}", orderStatusChanged, e.InnerException?.Message ?? e.Message);
                throw;
            }
        }
    }
}