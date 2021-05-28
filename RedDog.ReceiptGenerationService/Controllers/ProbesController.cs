using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RedDog.ReceiptGenerationService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProbesController : ControllerBase
    {
        private string DaprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500";
        private ILogger<ProbesController> _logger;
        private HttpClient _httpClient;

        public ProbesController(ILogger<ProbesController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("ready")]
        public async Task<IActionResult> IsReady()
        {
            return await Task.FromResult(Ok());
        }

        [HttpGet("healthz")]
        public async Task<IActionResult> IsHealthy()
        {
            // Ensure dapr sidecar is running and healthy. If not, fail the health check and have the pod restarted.
            // This should prevent the case where the application container is running before dapr is installed in
            // the case of a gitops deploy.
            var response = await _httpClient.GetAsync($"http://localhost:{DaprHttpPort}/v1.0/healthz");
            return new StatusCodeResult((int)response.StatusCode);
        }
    }
}