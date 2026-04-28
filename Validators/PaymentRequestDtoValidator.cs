using FluentValidation;
using MyEstore.DTOs;

namespace MyEstore.Validators;

public class PaymentRequestDtoValidator : AbstractValidator<PaymentRequestDto>
{
    public PaymentRequestDtoValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.PaymentGateway).NotEmpty();
        RuleFor(x => x.PaymentMethod).NotEmpty();
    }
}
