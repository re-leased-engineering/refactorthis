using RefactorThis.Application.Shared.Payments.DTOs;

namespace RefactorThis.Application.Shared.Payments;

public interface IPaymentService
{
    Task<string> ProcessPaymentAsync(ProcessPaymentDto paymentDto);
}