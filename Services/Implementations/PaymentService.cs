using Microsoft.EntityFrameworkCore;
using MyEstore.Data;
using MyEstore.DTOs;
using MyEstore.Exceptions;
using MyEstore.Models;
using MyEstore.Services.Interfaces;

namespace MyEstore.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext dbContext, ILogger<PaymentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto dto)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId)
            ?? throw new NotFoundException("Order not found.");

        if (dto.Amount <= 0)
        {
            throw new ValidationException("Payment amount must be positive.");
        }

        // Verify the submitted amount matches the actual order total — prevents undercharging
        if (dto.Amount != order.TotalAmount)
        {
            throw new ValidationException($"Payment amount does not match order total of {order.TotalAmount:F2}.");
        }

        var transactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

        var payment = new PaymentModel
        {
            OrderId = dto.OrderId,
            TransactionId = transactionId,
            PaymentGateway = dto.PaymentGateway,
            PaymentMethod = dto.PaymentMethod,
            Amount = dto.Amount,
            Status = "Completed",
            GatewayResponse = "Simulated payment success",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        await _dbContext.Payments.AddAsync(payment);
        order.PaymentStatus = "Paid";
        order.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Payment processed: {TransactionId} for order {OrderId}", transactionId, dto.OrderId);

        return new PaymentResponseDto
        {
            TransactionId = transactionId,
            Status = payment.Status,
            Amount = payment.Amount,
            Message = "Payment processed successfully."
        };
    }

    public async Task<bool> VerifyPaymentAsync(string transactionId)
    {
        var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId)
            ?? throw new NotFoundException("Transaction not found.");

        _logger.LogInformation("Payment verification attempted for {TransactionId}", transactionId);
        return payment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> RefundPaymentAsync(string transactionId)
    {
        var payment = await _dbContext.Payments.Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId)
            ?? throw new NotFoundException("Transaction not found.");

        if (!payment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Only completed payments can be refunded.");
        }

        payment.Status = "Refunded";
        payment.RefundAmount = payment.Amount;
        payment.RefundedAt = DateTime.UtcNow;

        if (payment.Order is not null)
        {
            payment.Order.PaymentStatus = "Refunded";
            payment.Order.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Payment refunded for {TransactionId}", transactionId);
        return true;
    }
}
