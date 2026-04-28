using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEstore.DTOs;
using MyEstore.Services.Interfaces;

namespace MyEstore.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    // POST api/payment/process
    [HttpPost("process")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _paymentService.ProcessPaymentAsync(dto);
        return Ok(result);
    }

    // GET api/payment/verify/{transactionId}
    [HttpGet("verify/{transactionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyPayment(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return BadRequest(new { message = "Transaction ID is required." });

        var verified = await _paymentService.VerifyPaymentAsync(transactionId);
        if (!verified)
            return NotFound(new { message = $"Transaction '{transactionId}' could not be verified." });

        return Ok(new { message = "Payment verified successfully.", transactionId });
    }

    // POST api/payment/refund/{transactionId}  — admin only
    [HttpPost("refund/{transactionId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundPayment(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            return BadRequest(new { message = "Transaction ID is required." });

        var refunded = await _paymentService.RefundPaymentAsync(transactionId);
        if (!refunded)
            return NotFound(new { message = $"Transaction '{transactionId}' not found or cannot be refunded." });

        return Ok(new { message = "Refund processed successfully.", transactionId });
    }
}
