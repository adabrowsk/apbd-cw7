using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication2.Models;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WarehouseController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] ProductWarehouseDTO input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = await _context.Products.FindAsync(input.IdProduct);
        if (product == null)
            return NotFound($"No product found with ID {input.IdProduct}");

        var warehouse = await _context.Warehouses.FindAsync(input.IdWarehouse);
        if (warehouse == null)
            return NotFound($"No warehouse found with ID {input.IdWarehouse}");

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.ProductId == input.IdProduct && o.Amount == input.Amount && o.CreatedAt <= input.CreatedAt);
        if (order == null)
            return BadRequest("No matching order found or order date is invalid.");

        if (order.FullfilledAt != null)
            return BadRequest("Order has already been fulfilled.");

        order.FullfilledAt = DateTime.Now;
        _context.Update(order);

        var productWarehouse = new ProductWarehouse
        {
            ProductId = input.IdProduct,
            WarehouseId = input.IdWarehouse,
            Amount = input.Amount,
            Price = product.Price * input.Amount,
            CreatedAt = DateTime.Now
        };
        _context.ProductWarehouses.Add(productWarehouse);
        await _context.SaveChangesAsync();

        return Ok(new { ProductWarehouseId = productWarehouse.Id });
    }
}