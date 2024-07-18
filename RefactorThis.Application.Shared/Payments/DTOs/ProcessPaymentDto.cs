namespace RefactorThis.Application.Shared.Payments.DTOs;

public class ProcessPaymentDto
{
    public int ReferenceKey { get; set; }
    public decimal Amount { get; set; }
}