using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
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
            if (order == null)
            {
                return NotFound();
            }

            order.CompletedDate = orderSummary.OrderCompletedDate;
            await dbContext.SaveChangesAsync();

            return Ok();
        }
        [HttpGet("/Orders/{period}/{timeSpan}")]
        public async Task<OrdersTimeSeries> GetOrderCountOverTime(string storeId, string period, string timeSpan, [FromServices] AccountingContext dbContext)
        {

            TimeSpan spanLength = XmlConvert.ToTimeSpan(timeSpan);
            var fromDate = DateTime.UtcNow.Subtract(spanLength);

            var totalOrders =       from o in dbContext.Orders
                                    where o.StoreId == storeId && o.PlacedDate > fromDate
                                    orderby o.PlacedDate descending
                                    group o by new StoreTimeSegmentMinute{
                                        StoreId = storeId,
                                        Year = o.PlacedDate.Year,
                                        Month = o.PlacedDate.Month,
                                        Day = o.PlacedDate.Day,
                                        Hour = o.PlacedDate.Hour,
                                        Minute = o.PlacedDate.Minute
                                    };

            var orderData =         from oi in totalOrders
                                    select new TimeSeries<int>
                                     {
                                        PointInTime = new DateTime(oi.Key.Year,oi.Key.Month, oi.Key.Day, oi.Key.Hour, oi.Key.Minute,0),
                                        Value = oi.Count()
                                    };

            var totalOrdersByMinute =  new OrdersTimeSeries{
                StoreId = storeId,
                Values =  await orderData.ToListAsync()
            };

            return totalOrdersByMinute;

        }


        [HttpGet("/Profit/{period}/{timeSpan}")]
        public OrdersTimeSeries GetProfitOverTime(string storeId, string period, string timeSpan, [FromServices] AccountingContext dbContext)
        {

            TimeSpan spanLength = XmlConvert.ToTimeSpan(timeSpan);
            var fromDate = DateTime.UtcNow.Subtract(spanLength);

            var totalOrders =       from o in dbContext.Orders
                                    where o.StoreId == storeId && o.PlacedDate > fromDate
                                    orderby o.PlacedDate descending
                                    group o by new StoreTimeSegmentMinute{
                                        StoreId = storeId,
                                        Year = o.PlacedDate.Year,
                                        Month = o.PlacedDate.Month,
                                        Day = o.PlacedDate.Day,
                                        Hour = o.PlacedDate.Hour,
                                        Minute = o.PlacedDate.Minute
                                    };

            var orderData =         from oi in totalOrders
                                    select new TimeSeries<int>
                                     {
                                        PointInTime = new DateTime(oi.Key.Year,oi.Key.Month, oi.Key.Day, oi.Key.Hour, oi.Key.Minute,0),
                                        Value = oi.Count()
                                    };

            var totalOrdersByMinute =  new OrdersTimeSeries{
                StoreId = storeId,
                Values =  orderData.ToList()
            };

            return totalOrdersByMinute;

        }

        [HttpGet("/OrderMetrics")]
        public async Task<List<OrderMetric>> GetOrderMetricsAsync(string storeId, [FromServices] AccountingContext dbContext)
        {
            var calcOrderItemsWithOrders = from o in dbContext.Orders
                                           join oi in dbContext.OrderItems on o.OrderId equals oi.OrderId
                                           select new
                                           {
                                               StoreId = o.StoreId,
                                               PlacedDate = o.PlacedDate,
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

            var calcOrderItemsByHour = from c in calcOrderItemsWithOrders
                                       group c by new { c.StoreId, OrderHour = c.PlacedDate.Hour }
                                       into g
                                       select new
                                       {
                                           StoreId = g.Key.StoreId,
                                           OrderHour = g.Key.OrderHour,
                                           OrderItemCount = g.Count(),
                                           TotalCost = g.Sum(i => i.TotalUnitCost),
                                           TotalPrice = g.Sum(i => i.TotalUnitPrice)
                                       };

            var calcOrders = from o in dbContext.Orders
                             where o.CompletedDate != null
                             select new
                             {
                                 OrderId = o.OrderId,
                                 StoreId = o.StoreId,
                                 PlacedDate = o.PlacedDate,
                                 CompletedDate = o.CompletedDate,
                                 FulfillmentTime = EF.Functions.DateDiffSecond(o.PlacedDate, o.CompletedDate)
                             };

            var calcOrdersByHour = from c in calcOrders
                                    group c by new { c.StoreId, Date = EF.Functions.DateFromParts(c.PlacedDate.Year, c.PlacedDate.Month, c.PlacedDate.Day), OrderHour = c.PlacedDate.Hour }
                                    into g
                                    select new
                                    {
                                        StoreId = g.Key.StoreId,
                                        OrderDate = g.Key.Date,
                                        OrderHour = g.Key.OrderHour,
                                        OrderCount = g.Count(),
                                        AverageFulfillmentTime = g.Average(i => i.FulfillmentTime)
                                    };

            var metrics = from o in calcOrdersByHour
                          join oi in calcOrderItemsByHour on new { o.StoreId, o.OrderHour } equals new { oi.StoreId, oi.OrderHour }
                          select new OrderMetric
                          {
                              StoreId = o.StoreId,
                              OrderDate = o.OrderDate,
                              OrderHour = o.OrderHour,
                              OrderCount = o.OrderCount,
                              AvgFulfillmentTimeSec = (int)o.AverageFulfillmentTime,
                              OrderItemCount = oi.OrderItemCount,
                              TotalCost = oi.TotalCost,
                              TotalPrice = oi.TotalPrice
                          };

            
            return await metrics.ToListAsync();
        }


        private IQueryable<IGrouping<StoreTimeSegmentMinute, Order>> GetOrdersByMinute(AccountingContext dbContext, string storeId, DateTime fromDate) {
            var totalOrders =       from o in dbContext.Orders
                                    where o.StoreId == storeId && o.PlacedDate > fromDate
                                    orderby o.PlacedDate descending
                                    group o by new StoreTimeSegmentMinute{
                                        StoreId = storeId,
                                        Year = o.PlacedDate.Year,
                                        Month = o.PlacedDate.Month,
                                        Day = o.PlacedDate.Day,
                                        Hour = o.PlacedDate.Hour,
                                        Minute = o.PlacedDate.Minute
                                    };
            return totalOrders;
        }

        private IQueryable<IGrouping<StoreTimeSegmentMinute, Order>> GetOrdersByHour(AccountingContext dbContext, string storeId, DateTime fromDate) {
            var totalOrders =       from o in dbContext.Orders
                                    where o.StoreId == storeId && o.PlacedDate > fromDate
                                    orderby o.PlacedDate descending
                                    group o by new StoreTimeSegmentMinute{
                                        StoreId = storeId,
                                        Year = o.PlacedDate.Year,
                                        Month = o.PlacedDate.Month,
                                        Day = o.PlacedDate.Day,
                                        Hour = o.PlacedDate.Hour
                                    };
            return totalOrders;
        }

        private IQueryable<IGrouping<StoreTimeSegmentMinute, Order>> GetOrdersByDay(AccountingContext dbContext, string storeId, DateTime fromDate) {
            var totalOrders =       from o in dbContext.Orders
                                    where o.StoreId == storeId && o.PlacedDate > fromDate
                                    orderby o.PlacedDate descending
                                    group o by new StoreTimeSegmentMinute{
                                        StoreId = storeId,
                                        Year = o.PlacedDate.Year,
                                        Month = o.PlacedDate.Month,
                                        Day = o.PlacedDate.Day
                                    };
            return totalOrders;
        }

        
    }
}
