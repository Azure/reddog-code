using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedDog.AccountingModel;
using RedDog.AccountingService.Models;

namespace RedDog.AccountingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountingController : ControllerBase
    {
        private const string PubSubName = "reddog.pubsub";
        private const string OrderTopic = "orders";
        private const string OrderCompletedTopic = "ordercompleted";
        private readonly ILogger<AccountingController> _logger;
        private readonly DaprClient _daprClient;

        public AccountingController(ILogger<AccountingController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [Topic(PubSubName, OrderTopic)]
        [HttpPost("orders")]
        public async Task<IActionResult> UpdateMetrics(OrderSummary orderSummary, [FromServices] AccountingContext dbContext)
        {
            _logger.LogInformation("Received Order Summary: {@OrderSummary}", orderSummary);

            Customer customer = dbContext.Customers.SingleOrDefault(c => c.LoyaltyId == orderSummary.LoyaltyId);
            customer ??= new Customer()
            {
                FirstName = orderSummary.FirstName,
                LastName = orderSummary.LastName,
                LoyaltyId = orderSummary.LoyaltyId
            };

            Order order = new Order()
            {
                OrderId = orderSummary.OrderId,
                StoreId = orderSummary.StoreId,
                PlacedDate = orderSummary.OrderDate,
                Customer = customer,
                OrderTotal = orderSummary.OrderTotal
            };

            foreach (var orderItemSummary in orderSummary.OrderItems)
            {
                order.OrderItems.Add(new OrderItem()
                {
                    ProductId = orderItemSummary.ProductId,
                    ProductName = orderItemSummary.ProductName,
                    Quantity = orderItemSummary.Quantity,
                    UnitCost = orderItemSummary.UnitCost,
                    UnitPrice = orderItemSummary.UnitPrice
                });
            }

            dbContext.Add(order);

            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [Topic(PubSubName, OrderCompletedTopic)]
        [HttpPost("ordercompleted")]
        public async Task<IActionResult> MarkOrderComplete(OrderSummary orderSummary, [FromServices] AccountingContext dbContext)
        {
            _logger.LogInformation("Received Completed Order Summary: {@OrderSummary}", orderSummary);

            Order order = dbContext.Orders.SingleOrDefault<Order>(o => o.OrderId == orderSummary.OrderId);
            if(order == null)
            {
                return NotFound();
            }

            order.CompletedDate = orderSummary.OrderCompletedDate;
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("OrderMetrics")]
        public async Task<List<OrderMetric>> GetOrderMetricsAsync()
        {
            return null;
        }
    }
}
