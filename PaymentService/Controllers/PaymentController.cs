using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private static readonly List<Payment> Payments = new();
        [HttpPost]
        public IActionResult ProcessPayment([FromBody] dynamic request)
        {
            int orderId = request.orderId;
            decimal amount = request.amount;
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = amount,
                Status = "Processed"
            };
            Payments.Add(payment);
            return Ok(payment);
        }
    }
}
