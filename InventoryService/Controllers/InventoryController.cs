using InventoryService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private static readonly List<Inventory> Inventories = new()
    {
        new Inventory { ProductId = "Product1", Stock = 100 },
        new Inventory { ProductId = "Product2", Stock = 200 }
    };
        [HttpPost("reserve")]
        public IActionResult ReserveInventory([FromBody] dynamic request)
        {
            string productId = request.productId;
            int quantity = request.quantity;
            var inventory = Inventories.FirstOrDefault(i => i.ProductId == productId);
            if (inventory == null || inventory.Stock < quantity)
            {
                return BadRequest("Insufficient stock.");
            }
            inventory.Stock -= quantity;
            return Ok();
        }
        [HttpPost("release")]
        public IActionResult ReleaseInventory([FromBody] dynamic request)
        {
            string productId = request.productId;
            int quantity = request.quantity;
            var inventory = Inventories.FirstOrDefault(i => i.ProductId == productId);
            if (inventory == null)
            {
                return NotFound();
            }

            inventory.Stock += quantity;
            return Ok();
        }
    }
}
