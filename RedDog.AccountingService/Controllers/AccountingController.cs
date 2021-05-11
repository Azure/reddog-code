using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("/OrderMetrics")]
        public async Task<List<OrderMetric>> GetOrderMetricsAsync(string storeId, [FromServices] AccountingContext dbContext)
        {
            var calcOrderItems = from oi in dbContext.OrderItems
                                 select new
                                 {
                                    OrderItemId = oi.OrderItemId,
                                    OrderId = oi.OrderId,
                                    ProductId = oi.ProductId,
                                    ProductName = oi.ProductName,
                                    Quantity = oi.Quantity,
                                    UnitCost = oi.UnitCost,
                                    UnitPrice = oi.UnitPrice,
                                    TotalUnitCost = oi.UnitCost * oi.Quantity,
                                    TotalUnitPrice = oi.UnitPrice * oi.Quantity
                                };

            var calcOrderItemsWithOrders = from o in dbContext.Orders
                                            join oi in calcOrderItems on o.OrderId equals oi.OrderId
                                            select new
                                            {
                                                Order = o,
                                                OrderItem = oi
                                            };

            var calcOrderItemsByHour = from c in calcOrderItemsWithOrders
                                       group c by new { c.Order.StoreId, OrderHour = c.Order.PlacedDate.Hour }
                                       into g
                                       select new
                                       {
                                            StoreId = g.Key.StoreId,
                                            OrderHour = g.Key.OrderHour,
                                            OrderItemCount = g.Count(),
                                            TotalCost = g.Sum(i => i.OrderItem.TotalUnitCost),
                                            TotalPrice = g.Sum(i => i.OrderItem.TotalUnitPrice)
                                       };

            // var calcOrderItemsByHour = calcOrderItems.Join()

            // var calcOrders = dbContext.Orders.Select(o => new
            // {
            //     OrderId = o.OrderId,
            //     StoreId = o.StoreId,
            //     PlacedDate = o.PlacedDate,
            //     CompletedDate = o.CompletedDate,
            //     FulfillmentTime = EF.Functions.DateDiffMinute(o.PlacedDate, o.CompletedDate)
            // });
// (select o.StoreId, cast(o.PlacedDate as date) PlacedDate, DATEPART(hh, o.PlacedDate) Hour, count(o.OrderId) OrderCount, avg(o.FullfillmentTime) AvgFullfillmentTime
// 	from
// 		(select StoreId, OrderId, PlacedDate, CompletedDate, DATEDIFF(s, PlacedDate, CompletedDate) FullfillmentTime
// 		from [Order]) o
// 	group by o.StoreId, cast(o.PlacedDate as date), DATEPART(hh, o.PlacedDate)) o
            var v = calcOrderItemsByHour.ToList();
            return null;
        }
    }
}
