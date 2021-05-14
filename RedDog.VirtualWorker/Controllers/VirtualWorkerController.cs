using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedDog.VirtualWorker.Models;

namespace RedDog.VirtualWorker.Controllers
{
    [ApiController]
    public class VirtualWorkerController : ControllerBase
    {
        private const string MakeLineServiceAppId = "make-line-service";
        public static readonly string StoreId = Environment.GetEnvironmentVariable("STORE_ID") ?? "Redmond";
        public static readonly int MinSecondsToCompleteItem = int.Parse(Environment.GetEnvironmentVariable("MIN_SECONDS_TO_COMPLETE_ITEM") ?? "2");
        public static readonly int MaxSecondsToCompleteItem = int.Parse(Environment.GetEnvironmentVariable("MAX_SECONDS_TO_COMPLETE_ITEM") ?? "9");
        private static readonly object ItemLock = new object();
        private static bool IsMakingItems = false;
        private readonly ILogger<VirtualWorkerController> _logger;
        private readonly DaprClient _daprClient;
        private readonly Random _random;

        public VirtualWorkerController(ILogger<VirtualWorkerController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
            _random = new Random();
        }

        [HttpPost]
        [Route("/orders")]
        public async Task<IActionResult> CheckOrders()
        {
            if (!IsMakingItems)
            {
                _logger.LogInformation($"The VirtualWorker ({StoreId}) is checking orders on the make line...");

                var orders = await GetOrders();

                _logger.LogInformation($"There {(orders.Count == 1 ? "is" : "are")} {orders.Count} {(orders.Count == 1 ? "order" : "orders")} waiting to be made.");

                if (orders.Count > 0)
                {
                    lock (ItemLock)
                    {
                        IsMakingItems = true;
                    }

                    try
                    {
                        while (orders.Count > 0)
                        {
                            var order = orders.First();

                            _logger.LogInformation($"The VirtualWorker ({StoreId}) is making an order for {order.FirstName} {order.LastName}...");

                            foreach (var orderItem in order.OrderItems)
                            {
                                _logger.LogInformation($"The VirtualWorker ({StoreId}) is making {orderItem.Quantity} {orderItem.ProductName}.");

                                await Task.Delay(_random.Next(MinSecondsToCompleteItem * 1000, MaxSecondsToCompleteItem * 1000));

                                _logger.LogInformation($"The VirtualWorker ({StoreId}) completed {orderItem.Quantity} {orderItem.ProductName}.");
                            }

                            await CompleteOrder(order);
                            orders.RemoveAt(0);

                            _logger.LogInformation($"{order.FirstName} {order.LastName}, your order is ready!");
                        }
                    }
                    finally
                    {
                        lock (ItemLock)
                        {
                            IsMakingItems = false;
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"The make line is empty! Time to drum up some customers!");

                    await Task.Delay(5000);
                }
            }

            return Ok();
        }

        private async Task<List<OrderSummary>> GetOrders()
        {
            try
            {
                return await _daprClient.InvokeMethodAsync<List<OrderSummary>>(HttpMethod.Get, MakeLineServiceAppId, $"orders/{StoreId}");
            }
            catch(Exception e)
            {
                _logger.LogError("Error invoking make line service to retrieve orders. Message: {Message}",  e.InnerException?.Message ?? e.Message);
                return new List<OrderSummary>();
            }
        }

        private async Task CompleteOrder(OrderSummary orderSummary)
        {
            try
            {
                await _daprClient.InvokeMethodAsync<OrderSummary>(HttpMethod.Delete, MakeLineServiceAppId, $"orders/{orderSummary.StoreId}/{orderSummary.OrderId}", orderSummary);
            }
            catch(Exception e)
            {
                _logger.LogError("Error invoking make line service to complete order. Message: {Message}",  e.InnerException?.Message ?? e.Message);
            }
        }
    }
}