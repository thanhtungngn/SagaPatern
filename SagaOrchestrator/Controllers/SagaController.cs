using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SagaOrchestrator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SagaController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseInventoryUrl;
        private readonly string _baseOrderUrl;
        private readonly string _basePaymentUrl;
        public SagaController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _baseInventoryUrl = "https://localhost:7292/api/inventory";
            _baseOrderUrl = "https://localhost:7292/api/order";
            _basePaymentUrl = "https://localhost:7259/api/payment";
        }

        [HttpPost("ProcessOrder")]
        public async Task<IActionResult> ProcessOrder([FromBody] Order order)
        {
            order.Status = "Pending";
            // Step 1: Create Order
            var orderResponse = await CreateOrder(order);
            if (!orderResponse.IsSuccessStatusCode)
            {
                return BadRequest("Order creation failed.");
            }

            // Step 2: Reserve Inventory
            var inventoryResponse = await ReserveInventory(order);
            if (!inventoryResponse.IsSuccessStatusCode)
            {
                await CancelOrder(order.Id);
                return BadRequest("Inventory reservation failed.");
            }
            // Step 3: Process Payment
            var paymentResponse = await ProcessPayment(order);
            if (!paymentResponse.IsSuccessStatusCode)
            {
                await ReleaseInventory(order.ProductId, order.Quantity);
                await CancelOrder(order.Id);
                return BadRequest("Payment processing failed.");
            }
            order.Status = "Completed";
            return Ok(order);
        }

        //Service layer

        private async Task<HttpResponseMessage> CreateOrder(Order order)
        {
            var client = _httpClientFactory.CreateClient();
            return await client.PostAsJsonAsync(_baseOrderUrl, order);
        }
        private async Task<HttpResponseMessage> ReserveInventory(Order order)
        {
            var client = _httpClientFactory.CreateClient();
            return await client.PostAsJsonAsync($"{_baseInventoryUrl}/reserve", new { order.ProductId, order.Quantity });
        }
        private async Task<HttpResponseMessage> ProcessPayment(Order order)
        {
            var client = _httpClientFactory.CreateClient();
            return await client.PostAsJsonAsync(_basePaymentUrl, new { order.Id, Amount = order.Quantity * 10 });
        }
        private async Task<HttpResponseMessage> CancelOrder(int orderId)
        {
            var client = _httpClientFactory.CreateClient();
            return await client.DeleteAsync($"{_baseOrderUrl}/{orderId}");
        }
        private async Task<HttpResponseMessage> ReleaseInventory(string productId, int quantity)
        {
            var client = _httpClientFactory.CreateClient();
            return await client.PostAsJsonAsync($"{_baseInventoryUrl}/release", new { productId, quantity });
        }

    }

    public class Order
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
    }
}
