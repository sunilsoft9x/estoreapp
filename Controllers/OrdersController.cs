using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEstore.DTOs;
using MyEstore.Services.Interfaces;

namespace MyEstore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    // POST api/orders
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var order = await _orderService.CreateOrderAsync(userId.Value, dto);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderNumber }, order);
    }

    // GET api/orders/my?page=1&pageSize=20
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var orders = await _orderService.GetUserOrdersAsync(userId.Value);
        return Ok(orders);
    }

    // GET api/orders/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var isAdmin = User.IsInRole("Admin");
        var order = await _orderService.GetOrderByIdAsync(id, userId.Value, isAdmin);
        return Ok(order);
    }

    // GET api/orders?page=1&pageSize=20  — admin only
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var orders = await _orderService.GetAllOrdersAsync(page, pageSize);
        return Ok(orders);
    }

    // PATCH api/orders/{id}/status  — admin only
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromQuery] string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return BadRequest(new { message = "Status cannot be empty." });

        var updated = await _orderService.UpdateOrderStatusAsync(id, status);
        if (!updated)
            return NotFound(new { message = $"Order {id} not found." });

        return NoContent();
    }

    // DELETE api/orders/{id}/cancel
    [HttpDelete("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var isAdmin = User.IsInRole("Admin");
        var cancelled = await _orderService.CancelOrderAsync(id, userId.Value, isAdmin);
        if (!cancelled)
            return NotFound(new { message = $"Order {id} not found or cannot be cancelled." });

        return NoContent();
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    private int? GetCurrentUserId()
    {
        var value = User.FindFirstValue("id");
        return int.TryParse(value, out var id) ? id : null;
    }
}
