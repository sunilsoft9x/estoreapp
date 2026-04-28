using MyEstore.DTOs;

namespace MyEstore.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto dto);
    Task<bool> VerifyPaymentAsync(string transactionId);
    Task<bool> RefundPaymentAsync(string transactionId);
}
