using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedDog.VirtualCustomers.Models;

namespace RedDog.VirtualCustomers
{
    public class VirtualCustomers : BackgroundService
    {
        private const string OrderServiceDaprId = "order-service";
        private const string StoreId = "Redmond";
        public static readonly int maxItemQuantity = int.Parse(Environment.GetEnvironmentVariable("MAX_ITEM_QUANTITY") ?? "4");
        public static readonly int minSecondsToPlaceOrder = int.Parse(Environment.GetEnvironmentVariable("MIN_SEC_TO_PLACE_ORDER") ?? "8");
        public static readonly int maxSecondsToPlaceOrder = int.Parse(Environment.GetEnvironmentVariable("MAX_SEC_TO_PLACE_ORDER") ?? "15");
        public static readonly int minSecondsBetweenOrders = int.Parse(Environment.GetEnvironmentVariable("MIN_SEC_BETWEEN_ORDERS") ?? "5");
        public static readonly int maxSecondsBetweenOrders = int.Parse(Environment.GetEnvironmentVariable("MAX_SEC_BETWEEN_ORDERS") ?? "20");
        public static readonly int numOrders = int.Parse(Environment.GetEnvironmentVariable("NUM_ORDERS") ?? "-1");

        private readonly ILogger<VirtualCustomers> _logger;
        private readonly DaprClient _daprClient;
        private Task _ordersTask = Task.CompletedTask;
        private List<Product> _products;
        private Random _random;

        private static readonly (int, string, string)[] customers = {
            (1, "Bruce", "Wayne"),
            (2, "Robin", "Hood"),
            (3, "Don", "Diego de la Vega"),
            (4, "Barry", "Allen"),
            (5, "Michael", "Knight"),
            (6, "Angus", "MacGyver"),
            (7, "Robert", "McCall"),
            (8, "John 'Hannibal'", "Smith"),
            (9, "H.M. 'Howlin Mad'", "Murdock"),
            (10, "B.A.", "Baracus"),
            (11, "Templeton 'Faceman'", "Peck"),
            (12, "Peter", "Parker"),
            (13, "Leonardo", "Hamato"),
            (14, "Donatello", "Hamato"),
            (15, "Raphael", "Hamato"),
            (16, "Michelangelo", "Hamato"),
            (17, "Hamato", "Splinter"),
            (18, "Frank", "Castle"),
            (19, "Matt", "Murdock"),
            (20, "Harry", "Callahan")
        };

        public VirtualCustomers(ILogger<VirtualCustomers> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
            _random = new Random();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"The customers are ready to place their orders!");

            stoppingToken.Register(() =>
            {
                _ordersTask.Wait();
                _logger.LogInformation($"The remaining customers have cancelled their orders and are leaving the store.");
                Environment.Exit(1);
            });

            int ordersCreated = 0;
            do
            {
                try
                {
                    _products = await _daprClient.InvokeMethodAsync<List<Product>>(HttpMethod.Get, OrderServiceDaprId, "product");
                }
                catch(Exception e)
                {
                    _logger.LogError("Error retrieving products. Retrying in 5 seconds. Message: {Message}", e.InnerException?.Message ?? e.Message);
                    await Task.Delay(5000);
                }
            } while (!stoppingToken.IsCancellationRequested && _products == null);

            do
            {
                await Task.Delay(_random.Next(minSecondsBetweenOrders, maxSecondsBetweenOrders + 1) * 1000);
                await CreateOrder(stoppingToken);
                ordersCreated += 1;

            } while (!stoppingToken.IsCancellationRequested && (ordersCreated < numOrders || numOrders == -1));

            _logger.LogInformation($"The {(numOrders == 1 ? "customer has" : "customers have")} finished placing {numOrders} {(numOrders == 1 ? "order" : "orders")}.");
        }

        public async Task CreateOrder(CancellationToken stoppingToken)
        {
            CustomerOrder order = new CustomerOrder();
            int custNum = _random.Next(0, customers.Length);
            _logger.LogInformation("Customer {0} {1} with loyalty id {2} is placing an order.", customers[custNum].Item2, customers[custNum].Item3, customers[custNum].Item1);
            order.StoreId = StoreId;
            order.FirstName = customers[custNum].Item2;
            order.LastName = customers[custNum].Item3;
            order.LoyaltyId = customers[custNum].Item1.ToString();

            // Get total number of menu items (t) and ramdomly choose a number of them to order (n)
            int numProducts = _products.Count;
            int productNum = _random.Next(1, numProducts + 1); // Never order 0 items

            // Randomly choose a number between (1-t), (n) times to get a random quantity of (n) items
            List<CustomerOrderItem> orderItems = new List<CustomerOrderItem>();
            List<int> productIndicies = new List<int>();
            for (int i = 0; i < productNum; i++)
            {
                int productIndex = _random.Next(1, numProducts + 1);
                while (productIndicies.Contains(productIndex)) // Ensure no duplicate menu items within order
                {
                    productIndex = _random.Next(1, numProducts + 1);
                }

                productIndicies.Add(productIndex);
                var menuItem = _products.Find(x => x.ProductId == productIndex); // Assuming menuIds are numbered #1 to #menuItemTotal
                int quantity = _random.Next(1, maxItemQuantity + 1);

                orderItems.Add(new CustomerOrderItem
                {
                    ProductId = productIndex,
                    Quantity = quantity
                });
            }

            order.OrderItems = orderItems;
            if (!stoppingToken.IsCancellationRequested)
            {
                var request = _daprClient.CreateInvokeMethodRequest<CustomerOrder>(OrderServiceDaprId, "order", order);
                var response = await _daprClient.InvokeMethodWithResponseAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Order was unsuccessful: {0} {1} {2}", (int)response.StatusCode, response.StatusCode, await response.Content.ReadAsStringAsync());
                }
                else
                {
                    _ordersTask = Task.Delay(_random.Next(minSecondsToPlaceOrder, maxSecondsToPlaceOrder + 1) * 1000, stoppingToken); // Delay until next customer in the line id ready to order
                    await _ordersTask;
                    _logger.LogInformation("Customer {0} {1} has placed their order containing {2} items.", order.FirstName, order.LastName, order.OrderItems.Count);
                }
            }
        }
    }
}