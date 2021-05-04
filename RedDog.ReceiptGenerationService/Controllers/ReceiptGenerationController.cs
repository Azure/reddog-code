using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedDog.ReceiptGenerationService.Models;

namespace RedDog.ReceiptGenerationService.Controllers
{
    [ApiController]
    public class ReceiptGenerationConsumerController : ControllerBase
    {
        private const string OrderTopic = "orders";
        private const string PubSubName = "reddog.pubsub";
        private const string ReceiptBindingName = "reddog.binding.receipt";
        private readonly ILogger<ReceiptGenerationConsumerController> _logger;

        public ReceiptGenerationConsumerController(ILogger<ReceiptGenerationConsumerController> logger)
        {
            _logger = logger;
        }

        [Topic(PubSubName, OrderTopic)]
        [HttpPost("orders")]
        public async Task<IActionResult> GenerateReceipt(OrderSummary orderSummary, [FromServices] DaprClient daprClient)
        {
            _logger.LogInformation("Writing Order Summary (receipt) to storage: {@OrderSummary}", orderSummary);

            try
            {
                Dictionary<string, string> metadata = new Dictionary<string, string>();
                metadata.Add("blobName", $"{orderSummary.OrderId}.json");
                await daprClient.InvokeBindingAsync<OrderSummary>(ReceiptBindingName, "create", orderSummary, metadata);
            }
            catch (Exception e)
            {
                _logger.LogError("Error saving receipt: {@OrderSummary}, Message: {Message}", orderSummary, e.Message);
                return Problem(e.Message, null, (int)HttpStatusCode.InternalServerError);
            }

            return Ok();
        }
    }
}
